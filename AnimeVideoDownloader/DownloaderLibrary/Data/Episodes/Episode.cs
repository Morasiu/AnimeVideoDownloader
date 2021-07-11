using System;
using System.Diagnostics;

namespace DownloaderLibrary.Data.Episodes {
	[DebuggerDisplay("{Number}: {Name} Downloaded: {IsDownloaded}")]
	public class Episode {
		public int Number { get; set; }

		public string Name { get; set; }

		public Uri EpisodeUri { get; set; }

		public Uri DownloadUri { get; set; }

		public bool IsDownloaded { get; set; }

		public bool IsIgnored { get; set; } = false;

		public string Path { get; set; }

		public long TotalBytes { get; set; }

		public EpisodeType EpisodeType { get; set; }
	}
}