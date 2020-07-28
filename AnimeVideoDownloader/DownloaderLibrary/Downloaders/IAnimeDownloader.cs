using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloaderLibrary.Episodes;

namespace DownloaderLibrary.Downloaders {
	public interface IAnimeDownloader : IDisposable {
		Task DownloadAllEpisodesAsync(IProgress<DownloadProgress> progress = null);

		Task DownloadEpisode(Episode episode, IProgress<DownloadProgress> progress = null);

		IEnumerable<Episode> GetEpisodes();
	}
}