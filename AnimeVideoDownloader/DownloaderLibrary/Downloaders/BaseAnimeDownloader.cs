using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Downloader;
using DownloaderLibrary.Drivers;
using DownloaderLibrary.Episodes;
using DownloaderLibrary.Helpers;
using DownloaderLibrary.Services.Mapping;
using OpenQA.Selenium.Remote;
using Serilog;

namespace DownloaderLibrary.Downloaders {
	public abstract class BaseAnimeDownloader : IDisposable {
		protected readonly Uri EpisodeListUri;
		protected readonly IProgress<DownloadProgressData> Progress;
		protected List<Episode> Episodes;
		protected readonly DownloaderConfig Config;
		protected RemoteWebDriver Driver;
		public event EventHandler<DownloadProgressData> ProgressChanged;

		protected BaseAnimeDownloader(Uri episodeListUri, DownloaderConfig config = null) {
			EpisodeListUri = episodeListUri;
			Progress = new Progress<DownloadProgressData>(p => ProgressChanged?.Invoke(this, p));
			Config = config ?? new DownloaderConfig();
			Episodes = new List<Episode>();

			Log.Logger = new LoggerConfiguration()
			             .WriteTo.File("logs.txt", rollingInterval: RollingInterval.Infinite, shared: true)
			             .CreateLogger();
		}

		public virtual async Task InitAsync() {
			if (!Config.Checkpoint.Exist(Config.DownloadDirectory)) {
				Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
			}
			else {
				Episodes = Config.Checkpoint.Load(Config.DownloadDirectory).ToList();
				CheckDownloadedEpisodes(Episodes);
			}

			Driver = await ChromeDriverFactory.CreateNewAsync().ConfigureAwait(false);
			Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
			Driver.Url = EpisodeListUri.AbsoluteUri;
		}

		public async Task DownloadAllEpisodesAsync() {
			var episodes = await GetEpisodesAsync().ConfigureAwait(false);
			foreach (var episode in episodes) {
				await TryDownloadEpisode(episode).ConfigureAwait(false);
			}
		}

		private async Task TryDownloadEpisode(Episode episode) {
			int tryCount = 0;
			while (true) {
				if (tryCount > 30) break;
				tryCount = 0;
				if (episode.IsDownloaded) {
					Progress.Report(
						new DownloadProgressData(episode.Number, 1.0, episode.TotalBytes, episode.TotalBytes));
					break;
				}

				if (episode.IsIgnored) {
					Progress.Report(
						new DownloadProgressData(episode.Number, 0, 0, 0));
					break;
				}

				if (!Config.ShouldDownloadFillers && episode.EpisodeType == EpisodeType.Filler) break;

				try {
					await DownloadEpisode(episode).ConfigureAwait(false);
				}
				catch (Exception e) {
					if (IsServerReturningForbidden(e) || IsConnectionTimeout(e) || IsServerReturningNotFound(e)) {
						episode.DownloadUri = null;
					}

					ReportError(e, episode);
					var random = new Random();
					await Task.Delay(random.Next(800, 1500)).ConfigureAwait(false);
					tryCount++;
				}
			}
		}

		private void ReportError(Exception e, Episode episode) {
			var error = $"Error ({e.GetType()}: {e.Message}) Trying again... Info ({e.Source})";
			Log.Error(e, "Error while downloading episode: {EpisodeNumber}.\nError: {Error}", episode.Number, error);
			Progress.Report(new DownloadProgressData(episode.Number, 0,
				error: error));
		}

		public async Task DownloadEpisode(Episode episode) {
			if (episode.IsDownloaded) {
				Progress?.Report(new DownloadProgressData(episode.Number, 1.0, episode.TotalBytes, episode.TotalBytes));
			}

			if (episode.DownloadUri is null) {
				if (episode.EpisodeUri is null)
					throw new InvalidOperationException($"{nameof(episode.EpisodeUri)} was null.");
				episode.DownloadUri = await GetEpisodeDownloadUrl(episode);
				Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
			}

			if (episode.Path == null) episode.Path = Path.Combine(Config.DownloadDirectory, $"{episode.Number}.mp4");
			
			var config = new DownloadConfiguration {
				CheckDiskSizeBeforeDownload = true,
				OnTheFlyDownload = false,
			};
			var downloader = new DownloadService(config);

			DateTime timeSinceLastSave = DateTime.Now;
			downloader.DownloadProgressChanged += (sender, args) => {
				var downloadProgressData = new DownloadProgressData(episode.Number,
					args.ProgressPercentage / 100,
					args.ReceivedBytesSize,
					args.TotalBytesToReceive,
					(long) args.BytesPerSecondSpeed);

				Progress.Report(downloadProgressData);
				if (DateTime.Now - timeSinceLastSave > SaveProgressConfig.SaveTime) {
					downloader.Package.SavePackage(episode.Path);
					timeSinceLastSave = DateTime.Now;
				}
			};

			downloader.DownloadFileCompleted += (sender, args) => {
				if (!args.Cancelled && args.Error == null) {
					episode.IsDownloaded = true;
					Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
					downloader.Package.Delete(episode.Path);
					downloader.Dispose();
				}
			};
			downloader.DownloadStarted += (sender, args) => {
				episode.TotalBytes = args.TotalBytesToReceive;
				Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
			};
			var package = downloader.Package.LoadPackage(episode.Path);

			if (package == null) {
				await downloader.DownloadFileTaskAsync(episode.DownloadUri.AbsoluteUri, episode.Path)
				                .ConfigureAwait(false);
			}
			else
				await downloader.DownloadFileTaskAsync(package).ConfigureAwait(false);

			episode.IsDownloaded = true;
			downloader.Package.Delete(episode.Path);
		}

		public async Task<IEnumerable<Episode>> GetEpisodesAsync() {
			if (Episodes.Count == 0) {
				var episodes = await GetAllEpisodesFromEpisodeListUrlAsync().ConfigureAwait(false);
				Episodes = episodes.OrderBy(e => e.Number).ToList();
				Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
			}

			return Episodes;
		}

		public void UpdateEpisode(int number, Action<Episode> update) {
			var episode = Episodes.SingleOrDefault(a => a.Number == number);
			update(episode);
			Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
		}
		
		protected abstract Task<List<Episode>> GetAllEpisodesFromEpisodeListUrlAsync();

		protected abstract Task<Uri> GetEpisodeDownloadUrl(Episode episode);

		public virtual void Dispose() {
			try {
				Driver.Close();
			}
			catch (Exception) {
				// ignored
			}
		}


		private void CheckDownloadedEpisodes(List<Episode> episodes) {
			foreach (var episode in episodes) {
				episode.IsDownloaded = IsFileDownloadCompleted(episode, episode.Path);
			}
		}

		private bool IsFileDownloadCompleted(Episode episode, string filePath) {
			if (!File.Exists(filePath)) return false;
			if (episode.TotalBytes == 0) return false;

			var downloadedFileTotalBytes = new FileInfo(filePath).Length;
			if (downloadedFileTotalBytes == episode.TotalBytes) {
				return true;
			}

			return false;
		}

		private static bool IsConnectionTimeout(Exception e) {
			return (e.GetBaseException() as SocketException)?.SocketErrorCode == SocketError.TimedOut;
		}

		private static bool IsServerReturningForbidden(Exception e) {
			return e.Message == "The remote server returned an error: (403) Forbidden.";
		}

		private static bool IsServerReturningNotFound(Exception e) {
			return e.Message.Contains("404");
		}
	}
}