using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DownloaderLibrary.Checkpoints;
using DownloaderLibrary.Downloaders;
using DownloaderLibrary.Episodes;

namespace DownloaderLibrary {
	public class DownloaderManager : IDisposable {
		private readonly BaseAnimeDownloader _downloader;

		public DownloaderManager(Uri episodeListUri, string downloadPath, IProgress<DownloadProgress> progress = null) {
			if (!Directory.Exists(downloadPath)) {
				Directory.CreateDirectory(downloadPath);
			}

			if (episodeListUri.AbsoluteUri.Contains("wbijam")) {
				_downloader = new WbijamAnimeDownloader(episodeListUri, downloadPath, new JsonCheckpoint(), progress);
			}
			else {
				throw new NotImplementedException("Site not implemented.");
			}
		}

		public async Task DownloadAllEpisodes() {
			await _downloader.DownloadAllEpisodesAsync();
		}

		public async Task DownloadEpisodeAsync(Episode episode) {
			await _downloader.DownloadEpisode(episode);
		}

		public IEnumerable<Episode> GetEpisodes() {
			return _downloader.GetEpisodes();
		}

		public void Dispose() {
			_downloader.Dispose();
		}
	}
}