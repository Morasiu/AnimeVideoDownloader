using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DownloaderLibrary.Data.Episodes;
using DownloaderLibrary.Data.EpisodeSources;
using DownloaderLibrary.Extensions;
using DownloaderLibrary.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DownloaderLibrary.Downloaders {
	public class ShindenDownloader : BaseAnimeDownloader {
		public ShindenDownloader(Uri episodeListUri, DownloaderConfig config = null) : base(episodeListUri, config) { }

		protected override Task<List<Episode>> GetAllEpisodesFromEpisodeListUrlAsync() {
			var list = new List<Episode>();
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

			AcceptAll(wait);

			IWebElement table = TryToGetTable(wait);

			var rows = table.FindElements(By.TagName("tr"));
			foreach (var row in rows) {
				var columns = row.FindElements(By.TagName("td"));
				var icons = columns[2].FindElements(By.TagName("i"));
				var isNotOnline = icons[0].GetAttribute("class") != "fa fa-fw fa-check";
				if (isNotOnline) continue;

				var isFiller = icons.Count > 1 && icons[1].GetAttribute("class") == "fa fa-facebook button-with-tip";
				var number = int.Parse(columns[0].Text);
				var name = columns[1].Text;
				var episodeUrl = new Uri(columns[5].FindElement(By.TagName("a")).GetAttribute("href"));
				var episode = new Episode {
					Number = number,
					Name = name,
					EpisodeType = isFiller ? EpisodeType.Filler : EpisodeType.Normal,
					EpisodeUri = episodeUrl
				};
				list.Add(episode);
			}

			list = list.OrderBy(a => a.Number).ToList();
			return Task.FromResult(list);
		}

		private static IWebElement TryToGetTable(WebDriverWait wait) {
			try {
				return wait.Until(
					ExpectedConditions.ElementExists(
						By.TagName("tbody")));
			}
			catch (WebDriverTimeoutException) {
				// Failure
			}

			try {
				return wait.Until(
					ExpectedConditions.ElementExists(
						By.XPath("/html/body/div[6]/div/article/section[2]/div[2]/table/tbody")));
			}
			catch (WebDriverTimeoutException e) {
				// Failure
				throw new WebDriverTimeoutException("Cannot load episode list", e);
			}
		}

		protected override async Task<Uri> GetEpisodeDownloadUrlAsync(Episode episode) {
			Driver.Url = episode.EpisodeUri.AbsoluteUri;
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
			RemoveFuckingAnnoyingAds();
			AcceptAll(wait);

			var table = GetTable(wait);
			var rows = table.FindElements(By.TagName("tr"));

			var episodeSources = new List<EpisodeSource>();
			foreach (var row in rows) {
				var columns = row.FindElements(By.TagName("td"));
				var spans = columns[2].FindElements(By.TagName("span"));
				var soundsLanguage = spans[1].GetAttribute("textContent");
				if (!soundsLanguage.Equals("japoński", StringComparison.InvariantCultureIgnoreCase)) continue;

				var providerName = columns[0].Text;
				var quality = columns[1].Text;
				var button = columns[5].FindElement(By.TagName("a"));
				if (providerName.ToLower().Contains("cda".ToLower())) {
					var episodeSource = new EpisodeSource {
						ProviderType = ProviderType.Cda,
						Quality = QualityParser.FromString(quality),
						Language = Language.PL,
						Button = button
					};
					episodeSources.Add(episodeSource);
				}
				else if (providerName.ToLower().Contains("Gdrive".ToLower())) {
					var episodeSource = new EpisodeSource {
						ProviderType = ProviderType.GDrive,
						Quality = QualityParser.FromString(quality),
						Language = Language.PL,
						Button = button
					};
					episodeSources.Add(episodeSource);
				} else if (providerName.ToLower().Contains("Vk".ToLower())) {
					var episodeSource = new EpisodeSource {
						ProviderType = ProviderType.Vk,
						Quality = QualityParser.FromString(quality),
						Language = Language.PL,
						Button = button
					};
					episodeSources.Add(episodeSource);
				}
			}

			if (episodeSources.Count == 0) throw new NullReferenceException("Episodes with chosen criteria not found");

			episodeSources = episodeSources
			                 .OrderByDescending(a => a.Language)
			                 .ThenByDescending(a => a.Quality)
			                 .ToList();

			Uri episodeUri = null;
			foreach (var episodeSource in episodeSources) {
				try {
					await TryClickPlayButton(episodeSource);

					IWebElement iframe;
					try {
						wait.Timeout = TimeSpan.FromSeconds(60);
						iframe = wait.Until(
							ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"player-block\"]/iframe")));
					}
					catch (WebDriverTimeoutException) {
						continue;
					}


					var src = iframe.GetAttribute("src");
					if (episodeSource.ProviderType == ProviderType.Cda) {
						src = $"{src}?wersja={episodeSource.Quality.GetDescription()}";
					}

					episodeSource.SourceUrl = src;
				}
				catch (InvalidOperationException) { }
			}


			foreach (var episodeSource in episodeSources) {
				try {
					episodeUri = await new ProviderFactory(Driver).GetProvider(episodeSource.ProviderType)
					                                              .GetVideoSourceAsync(episodeSource.SourceUrl);
					break;
				}
				catch (WebDriverTimeoutException) {
					Driver.Url = EpisodeListUri.AbsoluteUri;
				}
			}

			if (episodeUri == null) {
				throw new InvalidOperationException("Could not find any episode source to download");
			}

			return episodeUri;
		}

		private void RemoveFuckingAnnoyingAds() {
			Actions actions = new Actions(Driver);
			actions.Click().Build().Perform();
			try {
				Driver.RemoveElementById("spolSticky");
			}
			catch (WebDriverException) {
				// IGNORE
			}

			try {
				Driver.RemoveElementByClassName("ipprtcnt");
			}
			catch (WebDriverException) {
				// IGNORE
			}
			
			try {
				Driver.RemoveElementByClassName("ipprtcnt");
			}
			catch (WebDriverException) {
				// IGNORE
			}

			var iframes = Driver.FindElements(By.TagName("iframe"));

			foreach (var iframe in iframes) {
				try {
					if (iframe.GetAttribute("src").Contains("ads")) {
						Driver.RemoveElementById(iframe.GetAttribute("id"));
					}
				}
				catch (Exception) {
					// Failure
				}
			}

		}

		private void AcceptAdult(WebDriverWait wait) {
			wait.Timeout = TimeSpan.FromSeconds(3);
			try {
				var adult = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"plus18\"]/div/button")));
				adult.Click();
			}
			catch (WebDriverTimeoutException) { }
			finally {
				wait.Timeout = TimeSpan.FromSeconds(30);
			}
		}

		private async Task TryClickPlayButton(EpisodeSource episodeSource) {
			var tryNumber = 30;
			while (true) {
				tryNumber--;
				if (tryNumber == 0) {
					throw new InvalidOperationException("Couldn't click in play button");
				}

				try {
					RemoveFuckingAnnoyingAds();
					episodeSource.Button.Click();
					try {
						// Check if video is loading (5 second countdown)
						new WebDriverWait(Driver, TimeSpan.FromSeconds(2))
							.Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"circle-counter\"]")));
					}
					catch (WebDriverTimeoutException) {
						continue;
					}
					return;
				}
				catch (ElementClickInterceptedException) {
					await Task.Delay(500);
				}
			}
		}

		private static IWebElement GetTable(WebDriverWait wait) {
			IWebElement table;
			try {
				table = wait.Until(
					ExpectedConditions.ElementExists(
						By.XPath("/html/body/div[4]/div/article/section[3]/div/table/tbody")));
				return table;
			}
			catch (WebDriverTimeoutException) { }

			try {
				table = wait.Until(ExpectedConditions.ElementExists(By.CssSelector("section.box.episode-player-list > div > table > tbody")));
				return table;
			}
			catch (WebDriverTimeoutException) {
				throw new WebDriverTimeoutException("Cannot load episode providers list");
			}
		}

		private void AcceptCookies() {
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(3));
			TryClickCookies("//span[text()='Zaakceptuj wszystko']", wait);
		}

		private static void TryClickCookies(string xPath, WebDriverWait wait) {
			IWebElement cookies = null;
			try {
				cookies = wait.Until(ExpectedConditions.ElementExists(By.XPath(xPath)));
				cookies.Click();
			}
			catch (WebDriverTimeoutException) { }
			catch (ElementClickInterceptedException) {
				cookies?.Click();
			}
			catch (ElementNotInteractableException) {
				// IGNORE
			}
		}

		private void AcceptOtherCookies(WebDriverWait wait) {
			wait.Timeout = TimeSpan.FromSeconds(5);
			IWebElement cookies = null;
			try {
				cookies = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"cookie-bar\"]/p/a[1]")));
				cookies.Click();
			}
			catch (WebDriverTimeoutException) { }
			catch (ElementClickInterceptedException) {
				cookies?.Click();
			}
		}

		private void AcceptAll(WebDriverWait wait) {
			AcceptCookies();
			AcceptAdult(wait);
			AcceptOtherCookies(wait);
			
		}
		
		private void AcceptPrivacyPolicy(WebDriverWait wait) {
			wait.Timeout = TimeSpan.FromSeconds(5);
			IWebElement privacyPolicy = null;
			try {
				privacyPolicy = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@id=\"cookie-bar\"]/p/a[2]")));
				privacyPolicy.Click();
			}
			catch (WebDriverTimeoutException) { }
			catch (ElementClickInterceptedException) {
				privacyPolicy?.Click();
			}
		}
	}
}