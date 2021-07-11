using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DownloaderLibrary.Data.Episodes;
using DownloaderLibrary.Helpers;
using DownloaderLibrary.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DownloaderLibrary.Downloaders {
	public class WbijamAnimeDownloader : BaseAnimeDownloader {
		public WbijamAnimeDownloader(Uri episodeListUri, DownloaderConfig config = null) :
			base(episodeListUri, config) {
		}

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
					EpisodeUri = new Uri(row.FindElement(By.TagName("a")).GetProperty("href")),
					EpisodeType = EpisodeTypeChecker.GetType(row.FindElements(By.TagName("td"))[1].Text)
				};
				episodes.Add(episode);
			}

			return Task.FromResult(episodes);
		}

		protected override async Task<Uri> GetEpisodeDownloadUrl(Episode episode) {
			Driver.Url = episode.EpisodeUri.AbsoluteUri;
			var videoProviders = Driver.FindElements(By.ClassName("lista_hover"));
			var providerType = ProviderType.Cda;

			string providerUrl = null;
			foreach (var videoProvider in videoProviders) {
				var info = videoProvider.FindElements(By.TagName("td"));
				var providerName = info[2];
				if (providerName.Text == "cda") {
					var rel = info[4].FindElement(By.TagName("span")).GetAttribute("rel");
					providerUrl =
						new Uri($"{EpisodeListUri.Scheme}://{EpisodeListUri.Host}/odtwarzacz-{rel}.html").AbsoluteUri;
					break;
				}
			}

			if (string.IsNullOrWhiteSpace(providerUrl))
				throw new InvalidOperationException(
					$"Provider url ({providerType}) for episode [{episode.Name}] could not be found.");

			IWebElement cdaUrlButton;
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
			try {
				cdaUrlButton =
					wait.Until(ExpectedConditions.ElementExists(
						By.XPath("//*[@id=\"tresc_lewa\"]/center/strong/a[1]")));
			}
			catch (Exception e) {
				Console.WriteLine(e);
				throw;
			}

			var cdaVideoViewUrl = cdaUrlButton.GetAttribute("href");

			var provider = new ProviderFactory(Driver).GetProvider(providerType);
			
			return  await provider.GetVideoSourceAsync(providerUrl);
		}
	}
}