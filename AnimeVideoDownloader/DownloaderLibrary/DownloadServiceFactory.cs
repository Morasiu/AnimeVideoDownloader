using Downloader;

namespace DownloaderLibrary {
	public class DownloadServiceFactory {
		private static readonly DownloadConfiguration config =  new DownloadConfiguration {
			CheckDiskSizeBeforeDownload = true,
			OnTheFlyDownload = false,
		};

		public static DownloadService Create() {
			return new DownloadService(config);
		}
	}
}