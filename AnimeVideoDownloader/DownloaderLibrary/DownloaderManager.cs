using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using DownloaderLibrary.Checkpoints;
using DownloaderLibrary.Downloaders;
using DownloaderLibrary.Episodes;

namespace DownloaderLibrary {
	public class DownloaderManager : IDisposable {
		private readonly BaseAnimeDownloader _downloader;

		public DownloaderManager(Uri episodeListUri, string downloadPath, IProgress<DownloadProgress> progress = null,  DownloaderConfig config = null) {
			if (!Directory.Exists(downloadPath)) {
				Directory.CreateDirectory(downloadPath);
			}

			if (episodeListUri.AbsoluteUri.Contains("wbijam")) {
				_downloader = new WbijamAnimeDownloader(episodeListUri, downloadPath, new JsonCheckpoint(), progress, config);
			}
			else {
				throw new NotImplementedException("Site not implemented.");
			}
		}

		public async Task InitAsync() {
			await _downloader.InitAsync();
		}

		public async Task DownloadAllEpisodesAsync() {
			await _downloader.DownloadAllEpisodesAsync().ConfigureAwait(false);
		}

		public async Task DownloadEpisodeAsync(Episode episode) {
			await _downloader.DownloadEpisode(episode).ConfigureAwait(false);
		}

		public IEnumerable<Episode> GetEpisodes() {
			return _downloader.GetEpisodes();
		}

		public void Dispose() {
			_downloader.Dispose();
		}
	}
}