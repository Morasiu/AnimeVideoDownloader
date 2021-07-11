using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html;
using DownloaderLibrary.Data.Episodes;
using DownloaderLibrary.Helpers;
using DownloaderLibrary.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DownloaderLibrary.Downloaders {
	public class OgladajAnimeDownloader : BaseAnimeDownloader {
		public OgladajAnimeDownloader(Uri episodeListUri, DownloaderConfig config = null) :
			base(episodeListUri, config) {
		}

		protected override Task<List<Episode>> GetAllEpisodesFromEpisodeListUrlAsync() {
			var episodes = new List<Episode>();
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
			var table = wait.Until(
				driver => driver.FindElement(By.Id("episode_table")).FindElement(By.TagName("tbody")));
			var rows = table.FindElements(By.TagName("tr")).ToArray();
			for (var index = 0; index < rows.Length; index++) {
				var row = rows[index];

				// Episode doesn't have a player
				var columns = row.FindElements(By.TagName("td"));
				var player = columns[3];
				if (player.Text == "brak") continue;

				var onClick = player.FindElement(By.TagName("button")).GetAttribute("onclick");
				var cutEpisodeUrl = Regex.Split(EpisodeListUri.AbsoluteUri, "/episodes")[0];
				var episodeId = Regex.Match(onClick, "(\\d+)").Groups[0];
				var episodeUri = new Uri($"{cutEpisodeUrl}/player/{episodeId}");
				var name = wait.Until(driver => string.IsNullOrEmpty(columns[1].Text) ? null : columns[1].Text);
				var episode = new Episode {
					Number = index + 1,
					Name = name,
					EpisodeUri = episodeUri,
					// OgladajAnime does not support fillers listing
					EpisodeType = EpisodeType.Normal
				};
				episodes.Add(episode);
			}

			return Task.FromResult(episodes);
		}

		protected override async Task<Uri> GetEpisodeDownloadUrl(Episode episode) {
			Driver.Url = episode.EpisodeUri.AbsoluteUri;
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
			var episodeTable = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"animemenu_player\"]/div/table/tbody")));
			var rows = episodeTable.FindElements(By.TagName("tr"));
			string downloadUrl = string.Empty;
			for (int i = 1; i < rows.Count; i++) {
				var row = rows[i];
				var columns = row.FindElements(By.TagName("td"));
				var flag = columns[1].FindElement(By.TagName("img")).GetAttribute("alt");
				// ignore english episodes
				if (flag != "pl") continue;

				var providerName = columns[2].Text;
				if (providerName == "cda") {
					columns[4].Click();

					wait.Until(driver =>
						driver.FindElement(By.Id("playerLoader")).GetAttribute("style") == "display: none;");

					var player = Driver.FindElement(By.Id("player"));
					downloadUrl = player.GetAttribute("src");
					break;
				}

			}

			var providerType = ProviderType.Cda;
			if (string.IsNullOrEmpty(downloadUrl))
				throw new InvalidOperationException(
					$"Provider url ({providerType}) for episode [{episode.Name}] could not be found.");
			

			return await new ProviderFactory(Driver).GetProvider(providerType).GetVideoSourceAsync(downloadUrl);
		}
	}
}