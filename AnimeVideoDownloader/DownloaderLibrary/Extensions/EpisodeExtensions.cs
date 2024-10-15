using System.IO;
using DownloaderLibrary.Data.Episodes;

namespace DownloaderLibrary.Extensions {
	public static class EpisodeExtensions {
		public static bool IsFileDownloadCompleted(this Episode episode) {
			if (!File.Exists(episode.Path)) return false;
			if (episode.TotalBytes == 0) return false;

			var downloadedFileTotalBytes = new FileInfo(episode.Path).Length;
			if (downloadedFileTotalBytes == episode.TotalBytes) {
				return true;
			}

			return false;
		}
	}
}