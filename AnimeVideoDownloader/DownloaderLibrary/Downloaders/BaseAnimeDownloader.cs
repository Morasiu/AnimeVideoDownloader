using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloaderLibrary.Episodes;

namespace DownloaderLibrary.Downloaders {
	public abstract class BaseAnimeDownloader : IDisposable {
		protected readonly IProgress<DownloadProgress> Progress;
		protected readonly DownloaderConfig _config;

		protected BaseAnimeDownloader(IProgress<DownloadProgress> progress, DownloaderConfig config = null) {
			Progress = progress;
			_config = config ?? new DownloaderConfig();
		}

		public abstract Task InitAsync();

		public abstract Task DownloadAllEpisodesAsync();

		public abstract Task DownloadEpisode(Episode episode);

		public abstract IEnumerable<Episode> GetEpisodes();

		public abstract void Dispose();
	}
}