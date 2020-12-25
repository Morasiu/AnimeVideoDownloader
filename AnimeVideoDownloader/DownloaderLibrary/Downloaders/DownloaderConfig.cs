using System;
using System.IO;
using DownloaderLibrary.Checkpoints;
using DownloaderLibrary.Helpers;

namespace DownloaderLibrary.Downloaders {
	public class DownloaderConfig {
		public string DownloadDirectory { get; set; } = DirectoryHelper.GetDefaultDownloadFolderPath();
		public ICheckpoint Checkpoint { get; set; } = new JsonCheckpoint();
		public bool ShouldDownloadFillers { get; set; } = false;
	}
}