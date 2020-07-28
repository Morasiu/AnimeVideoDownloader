using System;

namespace DownloaderLibrary.Episodes {
	public class Episode {
		public int Number { get; set; }

		public string Name { get; set; }

		public Uri EpisodeUri { get; set; }

		public Uri DownloadUri { get; set; }

		public bool IsDownloaded { get; set; }

		public string Path { get; set; }
	}
}