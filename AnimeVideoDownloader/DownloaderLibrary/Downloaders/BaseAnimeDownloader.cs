using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Downloader;
using DownloaderLibrary.Data.Checkpoints;
using DownloaderLibrary.Data.Episodes;
using DownloaderLibrary.Drivers;
using DownloaderLibrary.Helpers;
using DownloaderLibrary.Services.Mapping;
using DownloaderLibrary.Settings;
using OpenQA.Selenium.Remote;
using Serilog;

namespace DownloaderLibrary.Downloaders {
	public abstract class BaseAnimeDownloader : IDisposable {
		protected readonly Uri EpisodeListUri;
		protected readonly IProgress<DownloadProgressData> Progress;
		protected Checkpoint Checkpoint;
		protected readonly DownloaderConfig Config;
		protected RemoteWebDriver Driver;
		protected readonly Dictionary<int, DownloadService> Downloaders = new Dictionary<int, DownloadService>();
		public event EventHandler<DownloadProgressData> ProgressChanged;

		protected BaseAnimeDownloader(Uri episodeListUri, DownloaderConfig config = null) {
			EpisodeListUri = episodeListUri;
			Progress = new Progress<DownloadProgressData>(p => ProgressChanged?.Invoke(this, p));
			Config = config ?? new DownloaderConfig();
			Checkpoint = new Checkpoint() {
				EpisodeListUrl = episodeListUri.ToString(),
			};
			
			Log.Logger = new LoggerConfiguration()
			             .WriteTo.File("logs.txt", rollingInterval: RollingInterval.Infinite, shared: true)
			             .CreateLogger();
		}

		public virtual async Task InitAsync() {
			if (!Config.CheckpointManager.Exist(Config.DownloadDirectory)) {
				Config.CheckpointManager.Save(Config.DownloadDirectory, Checkpoint);
			}
			else {
				Checkpoint = Config.CheckpointManager.Load(Config.DownloadDirectory);
				CheckDownloadedEpisodes(Checkpoint.Episodes);
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
			int tryCount = 1;
			while (true) {
				if (tryCount >= Retry.MaxTryCount) break;
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

					ReportError(e, episode, tryCount);
					var random = new Random();
					await Task.Delay(random.Next(800, 1500)).ConfigureAwait(false);
					tryCount++;
				}
			}
		}

		private void ReportError(Exception e, Episode episode, int tryCount) {
			var error = $"Error ({e.GetType()}: {e.Message}) Retry({tryCount}/{Retry.MaxTryCount}) - Info ({e.StackTrace})";
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
				Config.CheckpointManager.Save(Config.DownloadDirectory, Checkpoint);
			}

			if (episode.Path == null) episode.Path = Path.Combine(Config.DownloadDirectory, $"{episode.Number}.mp4");
			
			var config = new DownloadConfiguration {
				CheckDiskSizeBeforeDownload = true,
				OnTheFlyDownload = false,
			};
			var downloader = new DownloadService(config);
			Downloaders[episode.Number] = downloader;
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
					Config.CheckpointManager.Save(Config.DownloadDirectory, Checkpoint);
					downloader.Package.Delete(episode.Path);
					downloader.Dispose();
				}
				else if (args.Cancelled) {
					Downloaders.Remove(episode.Number);
				}
			};
			downloader.DownloadStarted += (sender, args) => {
				episode.TotalBytes = args.TotalBytesToReceive;
				Config.CheckpointManager.Save(Config.DownloadDirectory, Checkpoint);
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

		public void CancelDownload(int episodeNumber) {
			if (Downloaders.TryGetValue(episodeNumber, out var downloader)) {
				downloader?.CancelAsync();
			}
		}

		public async Task<IEnumerable<Episode>> GetEpisodesAsync() {
			if (!Checkpoint.Episodes.Any()) {
				var episodes = await GetAllEpisodesFromEpisodeListUrlAsync().ConfigureAwait(false);
				Checkpoint.Episodes = episodes.OrderBy(e => e.Number).ToList();
				Config.CheckpointManager.Save(Config.DownloadDirectory, Checkpoint);
			}

			return Checkpoint.Episodes;
		}

		public async Task<IEnumerable<Episode>> SyncEpisodeList() {
			var newEpisodes = await GetAllEpisodesFromEpisodeListUrlAsync().ConfigureAwait(false);
			foreach (var newEpisode in newEpisodes) {
				if (!Checkpoint.Episodes.Contains(newEpisode)) Checkpoint.Episodes.Add(newEpisode);
			}
			
			Config.CheckpointManager.Save(Config.DownloadDirectory, Checkpoint);
			return Checkpoint.Episodes;
		}

		public void UpdateEpisode(int number, Action<Episode> update) {
			var episode = Checkpoint.Episodes.SingleOrDefault(a => a.Number == number);
			update(episode);
			Config.CheckpointManager.Save(Config.DownloadDirectory, Checkpoint);
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


		private void CheckDownloadedEpisodes(IEnumerable<Episode> episodes) {
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