namespace DownloaderLibrary {
	public struct DownloadProgress {
		public DownloadProgress(int episodeNumber, double percent, long bytesReceived = 0, long totalBytes = 0,
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
	}
}