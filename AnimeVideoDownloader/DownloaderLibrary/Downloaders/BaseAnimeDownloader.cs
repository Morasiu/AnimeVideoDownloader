using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DownloaderLibrary.Drivers;
using DownloaderLibrary.Episodes;
using OpenQA.Selenium.Remote;

namespace DownloaderLibrary.Downloaders {
	public abstract class BaseAnimeDownloader : IDisposable {
		protected readonly Uri EpisodeListUri;
		protected readonly IProgress<DownloadProgressData> Progress;
		protected List<Episode> Episodes;
		protected readonly DownloaderConfig Config;
		protected RemoteWebDriver Driver;

		protected BaseAnimeDownloader(Uri episodeListUri, DownloaderConfig config = null) {
			EpisodeListUri = episodeListUri;
			Progress = new Progress<DownloadProgressData>(progress => ProgressChanged?.Invoke(this, progress));
			Config = config ?? new DownloaderConfig();
			Episodes = new List<Episode>();
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
			foreach (var episode in episodes) {
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
					Progress.Report(new DownloadProgressData(episode.Number, 0,
						error: $"Error ({e.GetType()}: {e.Message}) Trying again... Info ({e.Source})"));
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

						Progress.Report(new DownloadProgressData(
							episode.Number,
							(double) args.ProgressPercentage / 100,
							args.BytesReceived,
							args.TotalBytesToReceive,
							previousBytesPerSecond)
						);
					};
				}

				var filePath = Path.Combine(Config.DownloadDirectory, $"{episode.Number}.mp4");
				episode.Path = filePath;
				Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
				client.DownloadFileCompleted += (sender, args) => {
					if (args.UserState != tcs) {
						if (args.Error != null) {
							Progress?.Report(new DownloadProgressData(episode.Number, 0, 0, 0, 0, args.Error.Message));
							tcs.TrySetException(args.Error);
						}
						else if (args.Cancelled)
							tcs.TrySetCanceled();
						else {
							Progress?.Report(new DownloadProgressData(episode.Number,
								1,
								totalBytes,
								totalBytes,
								0));
							tcs.TrySetResult(null);
							episode.IsDownloaded = true;
							episode.TotalBytes = totalBytes;
							Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
						}
					}
				};

				if (File.Exists(filePath)) {
					if (episode.TotalBytes == 0) {
						client.OpenRead(episode.DownloadUri);
						episode.TotalBytes = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
					}


					if (IsFileDownloadCompleted(episode, filePath)) {
						Progress?.Report(new DownloadProgressData(
							episode.Number,
							1.0,
							totalBytes,
							totalBytes));
						episode.IsDownloaded = true;
						Config.Checkpoint.Save(Config.DownloadDirectory, Episodes);
						return;
					}
				}

				client.DownloadFileAsync(episode.DownloadUri, filePath);

				await tcs.Task.ConfigureAwait(false);
			}
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

		public event EventHandler<DownloadProgressData> ProgressChanged;

		private void CheckDownloadedEpisodes(List<Episode> episodes) {
			foreach (var episode in episodes) {
				if (episode.IsDownloaded) {
					if (episode.DownloadUri == null)
						episode.IsDownloaded = false;
					else {
						episode.IsDownloaded = IsFileDownloadCompleted(episode, episode.Path);
					}
				}
			}
		}

		protected bool IsFileDownloadCompleted(Episode episode, string filePath) {
			if (string.IsNullOrWhiteSpace(episode.DownloadUri.AbsoluteUri))
				throw new NullReferenceException($"{nameof(episode.DownloadUri)} was null");


			if (!File.Exists(filePath)) return false;

			var downloadedFileTotalBytes = new FileInfo(filePath).Length;
			if (downloadedFileTotalBytes == episode.TotalBytes) {
				return true;
			}

			return false;
		}
	}
}