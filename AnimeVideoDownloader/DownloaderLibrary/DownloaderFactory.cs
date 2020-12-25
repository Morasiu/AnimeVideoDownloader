﻿using System;
using System.IO;
using DownloaderLibrary.Downloaders;

namespace DownloaderLibrary {
	public class DownloaderFactory {
		public BaseAnimeDownloader GetDownloaderForSite(Uri episodeListUri, DownloaderConfig config = null) {
			if (config == null)
				config = new DownloaderConfig();

			if (!Directory.Exists(config.DownloadDirectory)) {
				Directory.CreateDirectory(config.DownloadDirectory);
			}

			if (episodeListUri.AbsoluteUri.Contains("wbijam")) {
				return new WbijamAnimeDownloader(episodeListUri, config);
			}
			else if (episodeListUri.AbsoluteUri.Contains("https://ogladajanime.pl/")) {
				return new OgladajAnimeDownloader(episodeListUri, config);
			}
			else {
				return null;
			}
		}
	}
}