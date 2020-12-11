using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DownloaderLibrary.Checkpoints;
using DownloaderLibrary.Episodes;
using DownloaderLibrary.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace DownloaderLibrary.Downloaders {
	public class WbijamAnimeDownloader : BaseAnimeDownloader {
		private readonly Uri _episodeListUri;
		private readonly string _downloadPath;
		private readonly ICheckpoint _checkpoint;
		private RemoteWebDriver _driver;
		private List<Episode> _episodes;
		private readonly WebClient _client;

		public WbijamAnimeDownloader(Uri episodeListUri, string downloadPath,
			ICheckpoint checkpoint, IProgress<DownloadProgress> progress, DownloaderConfig config = null) : base(
			progress, config) {
			_episodeListUri = episodeListUri;
			_downloadPath = downloadPath;
			_checkpoint = checkpoint;
			_episodes = new List<Episode>();
			_client = new WebClient();
		}

		public override async Task InitAsync() {
			if (!_checkpoint.Exist(_downloadPath)) {
				_checkpoint.Save(_downloadPath, _episodes);
			}
			else {
				_episodes = _checkpoint.Load(_downloadPath).ToList();
				await Task.Run(CheckDownloadedEpisodes);
			}

			new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
			var service = ChromeDriverService.CreateDefaultService();
			service.SuppressInitialDiagnosticInformation = true;
			service.HideCommandPromptWindow = true;

			var chromeOptions = new ChromeOptions();
#if !DEBUG
			chromeOptions.AddArgument("headless");
#endif
			try {
				_driver = await Task.Run(() => new ChromeDriver(service, chromeOptions) {
					Url = _episodeListUri.AbsoluteUri,
				});
			}
			catch (InvalidOperationException e) {
				throw new ChromeVersionException(e.Message);
			}
		}

		public override async Task DownloadAllEpisodesAsync() {
			var random = new Random();
			var episodes = GetEpisodes();
			foreach (var episode in episodes) {
				if (episode.IsDownloaded) {
					Progress.Report(
						new DownloadProgress(episode.Number, 1.0, episode.TotalBytes, episode.TotalBytes, 0));
					continue;
				}

				if (!_config.ShouldDownloadFillers && episode.EpisodeType == EpisodeType.Filler) continue;

				try {
					await DownloadEpisode(episode).ConfigureAwait(false);
				}
				catch (Exception e) {
					Progress.Report(new DownloadProgress(episode.Number, 0,
						error: $"Error ({e.GetType()}: {e.Message}) Trying again... Info ({e.Source})"));
					await Task.Delay(random.Next(800, 1500)).ConfigureAwait(false);
					await DownloadAllEpisodesAsync().ConfigureAwait(false);
				}
			}
		}

		public override async Task DownloadEpisode(Episode episode) {
			if (episode.IsDownloaded) {
				Progress?.Report(new DownloadProgress(episode.Number, 1.0, episode.TotalBytes, episode.TotalBytes));
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
							episode.TotalBytes = totalBytes;
							_checkpoint.Save(_downloadPath, _episodes);
						}
					}
				};

				if (File.Exists(filePath)) {
					if (episode.TotalBytes == 0) {
						client.OpenRead(episode.DownloadUri);
						episode.TotalBytes = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
					}


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

				await tcs.Task.ConfigureAwait(false);
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
						EpisodeUri = new Uri(row.FindElement(By.TagName("a")).GetProperty("href")),
						EpisodeType = EpisodeTypeChecker.GetType(row.FindElements(By.TagName("td"))[1].Text)
					};
					_episodes.Add(episode);
				}

				_checkpoint.Save(_downloadPath, _episodes);
			}

			return _episodes.OrderBy(e => e.Number);
		}

		public override void Dispose() {
			try {
				_driver.Close();
			}
			catch (Exception) {
				// ignored
			}

			_client.Dispose();
			_driver?.Dispose();
		}

		private bool IsFileDownloadCompleted(Episode episode, string filePath) {
			if (string.IsNullOrWhiteSpace(episode.DownloadUri.AbsoluteUri))
				throw new NullReferenceException($"{nameof(episode.DownloadUri)} was null");


			if (!File.Exists(filePath)) return false;

			var downloadedFileTotalBytes = new FileInfo(filePath).Length;
			if (downloadedFileTotalBytes == episode.TotalBytes) {
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
						episode.IsDownloaded = IsFileDownloadCompleted(episode, episode.Path);
					}
				}
			}
		}

		private Uri GetEpisodeDownloadUrl(Episode episode) {
			_driver.Url = episode.EpisodeUri.AbsoluteUri;
			var videoProviders = _driver.FindElements(By.ClassName("lista_hover"));
			var providerType = "cda";

			string providerUrl = null;
			foreach (var videoProvider in videoProviders) {
				var info = videoProvider.FindElements(By.TagName("td"));
				var providerName = info[2];
				if (providerName.Text == providerType) {
					var rel = info[4].FindElement(By.TagName("span")).GetAttribute("rel");
					providerUrl =
						new Uri($"{_episodeListUri.Scheme}://{_episodeListUri.Host}/odtwarzacz-{rel}.html").AbsoluteUri;
					break;
				}
			}

			if (string.IsNullOrWhiteSpace(providerUrl))
				throw new InvalidOperationException(
					$"Provider url ({providerType}) for episode [{episode.Name}] could not be found.");
			return GetCdaUrl(providerUrl);
		}

		private Uri GetCdaUrl(string providerUrl) {
			var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
			_driver.Url = providerUrl;
			// Example
			// https://ebd.cda.pl/640x406/336124079?wersja=1080p
			IWebElement cdaUrlButton;
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
			var cdaVideoViewUrl1080P = $"{cdaVideoViewUrl}?wersja=1080p";

			// Go to CDA page
			_driver.Url = cdaVideoViewUrl1080P;

			var source =
				wait.Until(ExpectedConditions.ElementExists(By.TagName("video")));

			var url = source.GetAttribute("src");
			return new Uri(url);
		}
	}
}