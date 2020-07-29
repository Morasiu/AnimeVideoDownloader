using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DownloaderLibrary.Episodes;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DownloaderLibrary.Downloaders {
	public class WbijamAnimeDownloader : IAnimeDownloader {
		private readonly Uri _episodeListUri;
		private readonly string _downloadPath;
		private readonly RemoteWebDriver _driver;
		private readonly List<Episode> _episodes;

		public WbijamAnimeDownloader(Uri episodeListUri, string downloadPath) {
			_episodeListUri = episodeListUri;
			_downloadPath = downloadPath;
			_episodes = new List<Episode>();

			var service = ChromeDriverService.CreateDefaultService();
			service.SuppressInitialDiagnosticInformation = true;
			service.HideCommandPromptWindow = true;
			var chromeOptions = new ChromeOptions();
			chromeOptions.AddArgument("headless");
			_driver = new ChromeDriver(service, chromeOptions) {
				Url = episodeListUri.AbsoluteUri,
			};
		}

		public async Task DownloadAllEpisodesAsync(IProgress<DownloadProgress> progress = null) {
			var random = new Random();
			var episodes = GetEpisodes();
			foreach (var episode in episodes) {
				if (episode.IsDownloaded) continue;
				try {
					await DownloadEpisode(episode, progress);
				}
				catch (Exception e) {
					progress.Report(new DownloadProgress(episode.Number, 0, error: $"Error ({e.Message}) Trying again..."));
					await Task.Delay(random.Next(800, 1500));
					await DownloadAllEpisodesAsync(progress);
				}
			}
		}

		public async Task DownloadEpisode(Episode episode, IProgress<DownloadProgress> progress = null) {
			if (episode.IsDownloaded) {
				progress.Report(new DownloadProgress(episode.Number, 1.0));
			}

			if (episode.DownloadUri is null) {
				if (episode.EpisodeUri is null)
					throw new InvalidOperationException($"{nameof(episode.EpisodeUri)} was null.");
				episode.DownloadUri = GetEpisodeDownloadUrl(episode);
			}

			DateTime lastUpdate = DateTime.Now;
			long lastBytes = 0;
			long totalBytes = 0;
			using (var client = new WebClient()) {
				long previousBytesPerSecond = 0;
				var tcs = new TaskCompletionSource<object>(episode.DownloadUri);
				if (progress != null) {
					client.DownloadProgressChanged += (sender, args) => {
						if (totalBytes == 0)
							totalBytes = args.TotalBytesToReceive;
						if (lastBytes == 0) {
							lastUpdate = DateTime.Now;
							lastBytes = args.BytesReceived;
							return;
						}

						var now = DateTime.Now;
						var timeSpan = now - lastUpdate;

						if (timeSpan.Milliseconds > 500) {
							var bytesChange = args.BytesReceived - lastBytes;
							var bytesPerSecond = bytesChange * 2;
							previousBytesPerSecond = bytesPerSecond;
							lastBytes = args.BytesReceived;
							lastUpdate = now;
						}

						progress.Report(new DownloadProgress(
							episode.Number,
							(double) args.ProgressPercentage / 100,
							args.BytesReceived,
							args.TotalBytesToReceive,
							previousBytesPerSecond)
						);
					};
				}

				var filePath = Path.Combine(_downloadPath, $"{episode.Number}.mp4");
				client.DownloadFileCompleted += (sender, args) => {
					if (args.UserState != tcs) {
						if (args.Error != null) {
							progress?.Report(new DownloadProgress(episode.Number, 0, 0, 0, 0, args.Error.Message));
							tcs.TrySetException(args.Error);
						}
						else if (args.Cancelled) tcs.TrySetCanceled();
						else {
							progress?.Report(new DownloadProgress(episode.Number,
								1,
								totalBytes,
								totalBytes,
								0));
							tcs.TrySetResult(null);
							episode.IsDownloaded = true;
						}
					}
				};


				if (File.Exists(filePath)) {
					client.OpenRead(episode.DownloadUri);
					var bytesTotal = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
					var downloadedFileTotalBytes = new FileInfo(filePath).Length;
					if (downloadedFileTotalBytes == bytesTotal) {
						progress.Report(new DownloadProgress(
							episode.Number,
							1.0,
							downloadedFileTotalBytes,
							downloadedFileTotalBytes));
						episode.IsDownloaded = true;
						return;
					}
				}

				client.DownloadFileAsync(episode.DownloadUri, filePath);

				await tcs.Task;
			}
		}

		public IEnumerable<Episode> GetEpisodes() {
			if (_episodes.Count == 0) {
				var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
				var table = wait.Until(driver => driver.FindElement(By.Id("tresc_lewa")));
				var rows = table.FindElements(By.TagName("tr")).Reverse().ToArray();
				for (var index = 0; index < rows.Length; index++) {
					var row = rows[index];
					var episode = new Episode {
						Number = index + 1,
						Name = row.FindElement(By.TagName("a")).Text,
						EpisodeUri = new Uri(row.FindElement(By.TagName("a")).GetProperty("href"))
					};
					_episodes.Add(episode);
				}
			}

			return _episodes.OrderBy(e => e.Number);
		}

		public void Dispose() {
			_driver?.Dispose();
		}

		private Uri GetEpisodeDownloadUrl(Episode episode) {
			_driver.Url = episode.EpisodeUri.AbsoluteUri;
			var videoProviders = _driver.FindElements(By.ClassName("lista_hover"));
			string mp4UpProviderUrl = null;
			foreach (var videoProvider in videoProviders) {
				var info = videoProvider.FindElements(By.TagName("td"));
				var providerName = info[2];
				if (providerName.Text == "mp4up") {
					var rel = info[4].FindElement(By.TagName("span")).GetAttribute("rel");
					mp4UpProviderUrl =
						new Uri($"{_episodeListUri.Scheme}://{_episodeListUri.Host}/odtwarzacz-{rel}.html").AbsoluteUri;
					break;
				}
			}

			if (string.IsNullOrWhiteSpace(mp4UpProviderUrl))
				throw new InvalidOperationException($"Mp4Up url for episode [{episode.Name}] could not be found.");
			var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
			_driver.Url = mp4UpProviderUrl;
			var iframe =
				wait.Until(ExpectedConditions.ElementExists(By.XPath("/html/body/div[2]/div[2]/center/iframe")));
			_driver.SwitchTo().Frame(iframe);
			var source =
				wait.Until(ExpectedConditions.ElementExists(By.XPath("/html/body/div[2]/div[1]/div[1]/button[5]")));
			var url = source.GetAttribute("href");
			return new Uri(url);
		}
	}
}