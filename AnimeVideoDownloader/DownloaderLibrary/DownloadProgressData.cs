﻿namespace DownloaderLibrary {
	public struct DownloadProgressData {
		public DownloadProgressData(int episodeNumber, double percent, long bytesReceived = 0, long totalBytes = 0,
			long bytesPerSecond = 0, string error = null) {
			EpisodeNumber = episodeNumber;
			Percent = percent;
			BytesReceived = bytesReceived;
			TotalBytes = totalBytes;
			BytesPerSecond = bytesPerSecond;
			Error = error;
		}

		public int EpisodeNumber { get; }
		public double Percent { get; }
		public long BytesReceived { get; }
		public long TotalBytes { get; }
		public long BytesPerSecond { get; }
		public string Error { get; }

		public static DownloadProgressData Done(int episodeNumber, long totalBytes) {
			return new DownloadProgressData(episodeNumber, 1.0, totalBytes, totalBytes);
		}
		
		public static DownloadProgressData Zero(int episodeNumber) {
			return new DownloadProgressData(episodeNumber, 0, 0, 0);
		}
		public static DownloadProgressData Start(int episodeNumber, long totalBytes) {
			return new DownloadProgressData(episodeNumber, 0, 0, totalBytes);
		}
	}
}