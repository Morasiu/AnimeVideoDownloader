using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DownloaderLibrary.Data.Episodes;
using DownloaderLibrary.Data.EpisodeSources;
using DownloaderLibrary.Extensions;
using DownloaderLibrary.Helpers;
using DownloaderLibrary.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DownloaderLibrary.Downloaders {
	public class WbijamAnimeDownloader : BaseAnimeDownloader {
		public WbijamAnimeDownloader(Uri episodeListUri, DownloaderConfig config = null) :
			base(episodeListUri, config) { }

		protected override Task<List<Episode>> GetAllEpisodesFromEpisodeListUrlAsync() {
			var episodes = new List<Episode>();
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
			var table = wait.Until(driver => driver.FindElement(By.Id("tresc_lewa")));
			var rows = table.FindElements(By.TagName("tr")).Reverse().ToArray();
			for (var index = 0; index < rows.Length; index++) {
				var row = rows[index];
				var episode = new Episode {
					Number = index + 1,
					Name = row.FindElement(By.TagName("a")).Text,
					EpisodeUri = new Uri(row.FindElement(By.TagName("a")).GetDomProperty("href")),
					EpisodeType = EpisodeTypeChecker.GetType(row.FindElements(By.TagName("td"))[1].Text)
				};
				episodes.Add(episode);
			}

			return Task.FromResult(episodes);
		}

		protected override async Task<Uri> GetEpisodeDownloadUrlAsync(Episode episode) {
			Driver.Url = episode.EpisodeUri.AbsoluteUri;
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

			AcceptCookies();

			var table = Driver.FindElement(By.TagName("table"));
			var providerType = ProviderType.Cda;

			var videoProviders = table.FindElements(By.TagName("tr"));
			var episodeSources = new List<EpisodeSource>();
			foreach (var videoProvider in videoProviders.Skip(1)) {
				var info = videoProvider.FindElements(By.TagName("td"));
				var language = GetLanguageFromIcon(info[0].FindElement(By.TagName("img")).GetAttribute("src"));
				var providerName = info[2];
				var link = info[4];
				// var rel = link.FindElement(By.TagName("span")).GetAttribute("rel");
				// var linkTo =
				// 	new Uri($"{EpisodeListUri.Scheme}://{EpisodeListUri.Host}/odtwarzacz-{rel}.html").AbsoluteUr
				if (providerName.Text == "cda") {
					var episodeSource = new EpisodeSource() {
						ProviderType = ProviderType.Cda,
						Quality = Quality.P1080,
						Language = language,
						Button = link
					};

					episodeSources.Add(episodeSource);
				}
			}


			if (episodeSources.Count == 0) {
				throw new InvalidOperationException(
					$"Provider url ({providerType}) for episode [{episode.Name}] could not be found.");
			}

			episodeSources = episodeSources
			                 .OrderByDescending(a => a.Language)
			                 .ThenByDescending(a => a.Quality)
			                 .ToList();

			Uri episodeUri = null;
			foreach (var episodeSource in episodeSources) {
				try {
					episodeSource.Button.Click();

					IWebElement iframe;
					try {
						wait.Timeout = TimeSpan.FromSeconds(60);
						iframe = wait.Until(
							ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"tresc_lewa\"]/center/iframe")));
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

		private void AcceptCookies() {
			try {
				var acceptButton = Driver.FindElement(By.XPath("//*[text()='Zaakceptuj wszystko']"));
				acceptButton.Click();
			}
			catch {
				// ignored
			}
		}

		private static Language GetLanguageFromIcon(string info) {
			if (info.Contains("pl")) {
				return Language.PL;
			}

			return Language.Unknown;
		}
	}
}