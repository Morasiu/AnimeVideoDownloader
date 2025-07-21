using Downloader;

namespace DownloaderLibrary {
	public class DownloadServiceFactory {
		private static readonly DownloadConfiguration config =  new DownloadConfiguration {
			CheckDiskSizeBeforeDownload = true,
			OnTheFlyDownload = false,
			RequestConfiguration = new RequestConfiguration
			{
				UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
			}
		};

		public static DownloadService Create() {
			return new DownloadService(config);
		}
	}
}