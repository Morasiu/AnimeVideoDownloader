using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DownloaderLibrary.Data.Episodes;
using DownloaderLibrary.Data.EpisodeSources;
using DownloaderLibrary.Extensions;
using DownloaderLibrary.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog.Core;

namespace DownloaderLibrary.Downloaders {
	public class ShindenDownloader : BaseAnimeDownloader {
		public ShindenDownloader(Uri episodeListUri, DownloaderConfig config = null) : base(episodeListUri, config) { }

		protected override Task<List<Episode>> GetAllEpisodesFromEpisodeListUrlAsync() {
			var list = new List<Episode>();
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
			AcceptCookies(wait);
			var table = wait.Until(a =>
				a.FindElement(By.XPath("/html/body/div[4]/div/article/section[2]/div[2]/table/tbody")));
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

		protected override async Task<Uri> GetEpisodeDownloadUrl(Episode episode) {
			var episodeSrcUrls = new List<string>();
			Driver.Url = episode.EpisodeUri.AbsoluteUri;
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
			AcceptCookies(wait);
			var table = GetTable(wait);
			var rows = table.FindElements(By.TagName("tr"));
			var episodeSources = new List<EpisodeSource>();
			foreach (var row in rows) {
				var columns = row.FindElements(By.TagName("td"));
				var providerName = columns[0].Text;

				if (providerName.ToLower().Contains("cda".ToLower())) {
					var spans = columns[2].FindElements(By.TagName("span"));
					var soundsLanguage = spans[1].GetAttribute("textContent");
					if (!soundsLanguage.Equals("japoński", StringComparison.InvariantCultureIgnoreCase)) continue;
					var quality = columns[1].Text;
					var button = columns[5].FindElement(By.TagName("a"));

					var episodeSource = new EpisodeSource {
						ProviderType = ProviderType.Cda,
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

			foreach (var episodeSource in episodeSources) {
				try {
					TryClickButton(episodeSource);

					IWebElement iframe;
					try {
						iframe = wait.Until(a =>
							a.FindElement(By.XPath("/html/body/div[4]/div/article/div[2]/div/iframe")));
					}
					catch (WebDriverTimeoutException) {
						continue;
					}

					var src = iframe.GetAttribute("src");
					var fullSrc = $"{src}?wersja={episodeSource.Quality.GetDescription()}";
					episodeSource.SourceUrl = fullSrc;
				}
				catch (InvalidOperationException) { }
			}

			Uri episodeUri = null;
			foreach (var episodeSource in episodeSources) {
				try {
					episodeUri = await new ProviderFactory(Driver).GetProvider(episodeSource.ProviderType)
					                                              .GetVideoSourceAsync(episodeSource.SourceUrl);
					break;
				}
				catch (WebDriverTimeoutException) { }
			}

			if (episodeUri == null) {
				throw new InvalidOperationException("Could not find any episode source to download");
			} 
			
			return episodeUri;
		}

		private static void TryClickButton(EpisodeSource episodeSource) {
			var tryNumber = 30;
			while (true) {
				tryNumber--;
				if (tryNumber == 0) {
					throw new InvalidOperationException("Couldn't click in button");
				}

				try {
					episodeSource.Button.Click();
					return;
				}
				catch (ElementClickInterceptedException) {
					Thread.Sleep(500);
				}
			}
		}

		private static IWebElement GetTable(WebDriverWait wait) {
			IWebElement table;
			try {
				table = wait.Until(a =>
					a.FindElement(By.XPath("/html/body/div[4]/div/article/section[3]/div/table/tbody")));
			}
			catch (WebDriverTimeoutException) {
				throw new WebDriverTimeoutException("Cannot load episode providers list");
			}

			return table;
		}

		private static void AcceptCookies(WebDriverWait wait) {
			wait.Timeout = TimeSpan.FromSeconds(5);
			try {
				var cookies = wait.Until(a =>
					a.FindElement(By.XPath("/html/body/div[14]/div[1]/div[2]/div/div[2]/button[2]")));
				cookies.Click();
				var cookies2 = wait.Until(a => a.FindElement(By.XPath("//*[@id=\"cookie-bar\"]/p/a[1]")));
				cookies2.Click();
			}
			catch (WebDriverTimeoutException) { }
			finally {
				wait.Timeout = TimeSpan.FromSeconds(30);
			}
		}
	}
}