using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloaderLibrary.Episodes;

namespace DownloaderLibrary.Downloaders {
	public abstract class BaseAnimeDownloader : IDisposable {
		protected readonly IProgress<DownloadProgress> Progress;

		protected BaseAnimeDownloader(IProgress<DownloadProgress> progress) {
			Progress = progress;
		}

		public abstract Task DownloadAllEpisodesAsync();

		public abstract Task DownloadEpisode(Episode episode);

		public abstract IEnumerable<Episode> GetEpisodes();

		public abstract void Dispose();
	}
}