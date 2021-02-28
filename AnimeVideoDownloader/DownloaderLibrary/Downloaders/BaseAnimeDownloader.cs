using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Downloader;
using DownloaderLibrary.Drivers;
using DownloaderLibrary.Episodes;
using DownloaderLibrary.Helpers;
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
			             .WriteTo.File("logs.txt", rollingInterval:RollingInterval.Infinite, shared:true)
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
			Driver.Url = EpisodeListUri.AbsoluteUri;
		}

		public async Task DownloadAllEpisodesAsync() {
			var random = new Random();
			var episodes = await GetEpisodesAsync().ConfigureAwait(false);
			foreach (var episode in episodes) 
			{
				if (episode.IsDownloaded) {
					Progress.Report(
						new DownloadProgressData(episode.Number, 1.0, episode.TotalBytes, episode.TotalBytes));
					continue;
				}

				if (!Config.ShouldDownloadFillers && episode.EpisodeType == EpisodeType.Filler) continue;

				try {
					await DownloadEpisode(episode).ConfigureAwait(false);
				}
				catch (Exception e) {
					var error = $"Error ({e.GetType()}: {e.Message}) Trying again... Info ({e.Source})";
					Log.Error(e, "Error while downloading episode: {EpisodeNumber}", episode.Number);
					Progress.Report(new DownloadProgressData(episode.Number, 0,
						error: error));
					await Task.Delay(random.Next(800, 1500)).ConfigureAwait(false);
					await DownloadAllEpisodesAsync().ConfigureAwait(false);
				}
			}
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

			var downloadService = new DownloadService(config);
			long previousReceivedBytes = 0;
			DateTime timeSinceLastSave = DateTime.Now;
			
			downloadService.DownloadProgressChanged += (sender, args) => {
				var downloadProgressData = new DownloadProgressData(episode.Number,
					args.ProgressPercentage / 100,
					args.ReceivedBytesSize,
					args.TotalBytesToReceive,
					(long) args.BytesPerSecondSpeed);

				Progress.Report(downloadProgressData);
				previousReceivedBytes = args.ReceivedBytesSize;
				if (DateTime.Now - timeSinceLastSave > SaveProgressConfig.SaveTime) {
					downloadService.Package.SavePackage(episode.Path);
					timeSinceLastSave = DateTime.Now;
				}
			};

			downloadService.DownloadFileCompleted += (sender, args) => {
				if (!args.Cancelled && args.Error == null) {
					downloadService.Package.Delete(episode.Path);
					episode.IsDownloaded = true;
				}
			};
			downloadService.DownloadStarted += (sender, args) => {
				episode.TotalBytes = args.TotalBytesToReceive;
				Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
			}; 
			var package = downloadService.Package.LoadPackage(episode.Path);
			if (package == null) {
				await downloadService.DownloadFileAsync(episode.DownloadUri.AbsoluteUri, episode.Path)
				                     .ConfigureAwait(false);
			}
			else
				await downloadService.DownloadFileAsync(package).ConfigureAwait(false);

			episode.IsDownloaded = true;
			downloadService.Package.Delete(episode.Path);
		}

		public async Task<IEnumerable<Episode>> GetEpisodesAsync() {
			if (Episodes.Count == 0) {
				var episodes = await GetAllEpisodesFromEpisodeListUrlAsync().ConfigureAwait(false);
				Episodes = episodes.OrderBy(e => e.Number).ToList();
				Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
			}

			return Episodes;
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

			Driver?.Dispose();
		}


		private void CheckDownloadedEpisodes(List<Episode> episodes) {
			foreach (var episode in episodes) {
				if (episode.IsDownloaded) {
					episode.IsDownloaded = IsFileDownloadCompleted(episode, episode.Path);
				}
			}
		}

		protected bool IsFileDownloadCompleted(Episode episode, string filePath) {
			if (!File.Exists(filePath)) return false;
			if (episode.TotalBytes == 0) return false;
			
			var downloadedFileTotalBytes = new FileInfo(filePath).Length;
			if (downloadedFileTotalBytes == episode.TotalBytes) {
				return true;
			}

			return false;
		}
	}
}