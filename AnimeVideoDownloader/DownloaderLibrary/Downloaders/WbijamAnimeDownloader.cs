using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DownloaderLibrary.Checkpoints;
using DownloaderLibrary.Episodes;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DownloaderLibrary.Downloaders {
	public class WbijamAnimeDownloader : BaseAnimeDownloader {
		private readonly Uri _episodeListUri;
		private readonly string _downloadPath;
		private readonly ICheckpoint _checkpoint;
		private readonly RemoteWebDriver _driver;
		private readonly List<Episode> _episodes;
		private readonly WebClient _client;

		public WbijamAnimeDownloader(Uri episodeListUri, string downloadPath, 
			ICheckpoint checkpoint, IProgress<DownloadProgress> progress) : base(progress) {
			_episodeListUri = episodeListUri;
			_downloadPath = downloadPath;
			_checkpoint = checkpoint;
			_episodes = new List<Episode>();
			_client = new WebClient();

			if (!checkpoint.Exist(downloadPath)) {
				checkpoint.Save(downloadPath, _episodes);
			} else {
				_episodes = checkpoint.Load(downloadPath).ToList();
				CheckDownloadedEpisodes();
			}

			var service = ChromeDriverService.CreateDefaultService();
			service.SuppressInitialDiagnosticInformation = true;
			service.HideCommandPromptWindow = true;
			var chromeOptions = new ChromeOptions();
			chromeOptions.AddArgument("headless");
			_driver = new ChromeDriver(service, chromeOptions) {
				Url = episodeListUri.AbsoluteUri,
			};

		}

		public override async Task DownloadAllEpisodesAsync() {
			var random = new Random();
			var episodes = GetEpisodes();
			foreach (var episode in episodes) {
				if (episode.IsDownloaded) {
					Progress.Report(new DownloadProgress(episode.Number, 1.0));
					continue;
				}

				try {
					await DownloadEpisode(episode);
				}
				catch (Exception e) {
					Progress.Report(new DownloadProgress(episode.Number, 0,
						error: $"Error ({e.Message}) Trying again..."));
					await Task.Delay(random.Next(800, 1500));
					await DownloadAllEpisodesAsync();
				}
			}
		}

		public override async Task DownloadEpisode(Episode episode) {
			if (episode.IsDownloaded) {
				Progress?.Report(new DownloadProgress(episode.Number, 1.0));
			}

			if (episode.DownloadUri is null) {
				if (episode.EpisodeUri is null)
					throw new InvalidOperationException($"{nameof(episode.EpisodeUri)} was null.");
				episode.DownloadUri = GetEpisodeDownloadUrl(episode);
				_checkpoint.Save(_downloadPath, _episodes);
			}

			DateTime lastUpdate = DateTime.Now;
			long lastBytes = 0;
			long totalBytes = 0;
			long previousBytesPerSecond = 0;
			var tcs = new TaskCompletionSource<object>(episode.DownloadUri);
			using (var client = new WebClient()) {
				if (Progress != null) {
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
							var bytesPerSecond = bytesChange * (1000 / 500);
							previousBytesPerSecond = bytesPerSecond;
							lastBytes = args.BytesReceived;
							lastUpdate = now;
						}

						Progress.Report(new DownloadProgress(
							episode.Number,
							(double) args.ProgressPercentage / 100,
							args.BytesReceived,
							args.TotalBytesToReceive,
							previousBytesPerSecond)
						);
					};
				}

				var filePath = Path.Combine(_downloadPath, $"{episode.Number}.mp4");
				episode.Path = filePath;
				_checkpoint.Save(_downloadPath, _episodes);
				client.DownloadFileCompleted += (sender, args) => {
					if (args.UserState != tcs) {
						if (args.Error != null) {
							Progress?.Report(new DownloadProgress(episode.Number, 0, 0, 0, 0, args.Error.Message));
							tcs.TrySetException(args.Error);
						}
						else if (args.Cancelled) 
							tcs.TrySetCanceled();
						else {
							Progress?.Report(new DownloadProgress(episode.Number,
								1,
								totalBytes,
								totalBytes,
								0));
							tcs.TrySetResult(null);
							episode.IsDownloaded = true;
							_checkpoint.Save(_downloadPath, _episodes);
						}
					}
				};
			
				if (File.Exists(filePath)) {
					if (IsFileDownloadCompleted(episode, filePath)) {
						Progress?.Report(new DownloadProgress(
							episode.Number,
							1.0,
							totalBytes,
							totalBytes));
						episode.IsDownloaded = true;
						_checkpoint.Save(_downloadPath, _episodes);
						return;
					}
				}

				client.DownloadFileAsync(episode.DownloadUri, filePath);

				await tcs.Task;
			}

		}

		public override IEnumerable<Episode> GetEpisodes() {
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

				_checkpoint.Save(_downloadPath, _episodes);
			}

			return _episodes.OrderBy(e => e.Number);
		}

		public override void Dispose() {
			_client.Dispose();
			_driver?.Dispose();
		}

		private bool IsFileDownloadCompleted(Episode episode, string filePath) {
			if (string.IsNullOrWhiteSpace(episode.DownloadUri.AbsoluteUri))
				throw new NullReferenceException($"{nameof(episode.DownloadUri)} was null");
			try {
				_client.OpenRead(episode.DownloadUri);
			}
			catch (Exception) {
				var random = new Random();
				Thread.Sleep(random.Next(800, 1500));
				return IsFileDownloadCompleted(episode, filePath);
			}
			var bytesTotal = Convert.ToInt64(_client.ResponseHeaders["Content-Length"]);

			var downloadedFileTotalBytes = new FileInfo(filePath).Length;
			if (downloadedFileTotalBytes == bytesTotal) {
				return true;
			}

			return false;
		}

		private void CheckDownloadedEpisodes() {
			foreach (var episode in _episodes) {
				if (episode.IsDownloaded) {
					if (episode.DownloadUri == null)
						episode.IsDownloaded = false;
					else {
						if (!File.Exists(episode.Path)) {
							episode.IsDownloaded = IsFileDownloadCompleted(episode, episode.Path);
						}
					}
				}
			}
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