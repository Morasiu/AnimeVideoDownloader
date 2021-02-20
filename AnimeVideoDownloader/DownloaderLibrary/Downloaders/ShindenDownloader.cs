using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DownloaderLibrary.Episodes;
using DownloaderLibrary.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace DownloaderLibrary.Downloaders {
	public class ShindenDownloader : BaseAnimeDownloader{
		public ShindenDownloader(Uri episodeListUri, DownloaderConfig config = null) : base(episodeListUri, config) { }
		protected override Task<List<Episode>> GetAllEpisodesFromEpisodeListUrlAsync() {
			var list = new List<Episode>();
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
			var table = wait.Until(a => a.FindElement(By.XPath("/html/body/div[4]/div/article/section[2]/div[2]/table/tbody")));
			var rows = table.FindElements(By.TagName("tr"));
			foreach (var row in rows) {
				var columns = row.FindElements(By.TagName("td"));
				var number = int.Parse(columns[0].Text);
				var name = columns[1].Text;
				var episodeUrl = new Uri(columns[5].FindElement(By.TagName("a")).GetAttribute("href"));
				var episode = new Episode {
					Number = number,
					Name = name,
					EpisodeType = EpisodeType.Normal,
					EpisodeUri = episodeUrl
				};
				list.Add(episode);
			}

			list = list.OrderBy(a => a.Number).ToList();
			return Task.FromResult(list);
		}

		protected override async Task<Uri> GetEpisodeDownloadUrl(Episode episode) {
			Driver.Url = episode.EpisodeUri.AbsoluteUri;
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
			var table = wait.Until(a => a.FindElement(By.XPath("/html/body/div[4]/div/article/section[3]/div/table/tbody")));
			var rows = table.FindElements(By.TagName("tr"));
			IWebElement playerButton = null; 
			foreach (var row in rows) {
				var columns = row.FindElements(By.TagName("td"));
				var providerName = columns[0].Text;
				if (providerName.Equals("cda", StringComparison.InvariantCultureIgnoreCase)) {
					var spans = columns[2].FindElements(By.TagName("span"));
					var soundsLanguage = spans[1].GetAttribute("textContent");
					if (!soundsLanguage.Equals("japoński", StringComparison.InvariantCultureIgnoreCase)) continue;

					playerButton = columns[5].FindElement(By.TagName("a"));
					break;
				}
			}

			if (playerButton == null) throw new NullReferenceException("Player button did not work");
			playerButton.Click();
			var iframe = wait.Until(a => a.FindElement(By.XPath("/html/body/div[4]/div/article/div[2]/div/iframe")));
			var src = iframe.GetAttribute("src");
			var provider = new ProviderFactory(Driver).GetProvider(ProviderType.Cda);
			return await provider.GetVideoSourceAsync(src);
		}
	}
}