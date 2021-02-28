using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DownloaderLibrary.Episodes;
using DownloaderLibrary.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

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
			Driver.Url = episode.EpisodeUri.AbsoluteUri;
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
			AcceptCookies(wait);
			IWebElement table;
			try {
				table = wait.Until(a =>
					a.FindElement(By.XPath("/html/body/div[4]/div/article/section[3]/div/table/tbody")));
			}
			catch (WebDriverTimeoutException) {
				throw new WebDriverTimeoutException("Cannot load episode providers list");
			}

			var rows = table.FindElements(By.TagName("tr"));
			IWebElement playerButton = null;
			var quality = string.Empty;
			foreach (var row in rows) {
				var columns = row.FindElements(By.TagName("td"));
				var providerName = columns[0].Text;
				if (providerName.Equals("cda", StringComparison.InvariantCultureIgnoreCase)) {
					var spans = columns[2].FindElements(By.TagName("span"));
					var soundsLanguage = spans[1].GetAttribute("textContent");
					if (!soundsLanguage.Equals("japoński", StringComparison.InvariantCultureIgnoreCase)) continue;
					quality = columns[1].Text;
					
					if (playerButton == null)
						playerButton = columns[5].FindElement(By.TagName("a"));
					if (quality == "1080p") {
						playerButton = columns[5].FindElement(By.TagName("a"));
						break;
					}
				}
			}

			if (playerButton == null) throw new NullReferenceException("Player button did not work");
			var tryNumber = 30;
			while (true) {
				tryNumber--;
				if (tryNumber == 0) break;
				try {
					playerButton.Click();
					break;
				}
				catch (ElementClickInterceptedException) {
					Thread.Sleep(1000);
				}
			}

			IWebElement iframe;
			try {
				iframe = wait.Until(a => a.FindElement(By.XPath("/html/body/div[4]/div/article/div[2]/div/iframe")));
			}
			catch (WebDriverTimeoutException) {
				throw new WebDriverTimeoutException("Cannot load episode player");
			}
			
			var src = iframe.GetAttribute("src");
			var fullSrc = $"{src}?wersja={quality}";
			var provider = new ProviderFactory(Driver).GetProvider(ProviderType.Cda);
			return await provider.GetVideoSourceAsync(fullSrc);
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