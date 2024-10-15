using DownloaderLibrary.CheckpointManagers;
using DownloaderLibrary.Helpers;

namespace DownloaderLibrary.Downloaders {
	public class DownloaderConfig {
		public string DownloadDirectory { get; set; } = DirectoryHelper.GetDefaultDownloadFolderPath();
		public ICheckpoint CheckpointManager { get; set; } = new JsonCheckpoint();
		public bool ShouldDownloadFillers { get; set; } = false;
	}
}