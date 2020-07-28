using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DownloaderLibrary.Downloaders;
using DownloaderLibrary.Episodes;

namespace DownloaderLibrary {
	public class DownloaderManager : IDisposable {
		private readonly IAnimeDownloader _downloader;

		public DownloaderManager(Uri episodeListUri, string downloadPath) {

			if (!Directory.Exists(downloadPath)) {
				Directory.CreateDirectory(downloadPath);
			}

			if (episodeListUri.AbsoluteUri.Contains("wbijam")) {
				_downloader = new WbijamAnimeDownloader(episodeListUri, downloadPath);
			}
		}

		public async Task DownloadAllEpisodes(Progress<DownloadProgress> progress = null) {
			await _downloader.DownloadAllEpisodesAsync(progress);
		}

		public async Task DownloadEpisodeAsync(Episode episode, Progress<DownloadProgress> progress = null) {
			await _downloader.DownloadEpisode(episode, progress);
		}

		public IEnumerable<Episode> GetEpisodes() {
			return _downloader.GetEpisodes();
		}

		public void Dispose() {
			_downloader.Dispose();
		}
	}
}