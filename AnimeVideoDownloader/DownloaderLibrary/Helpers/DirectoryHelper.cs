using System;
using System.IO;

namespace DownloaderLibrary.Helpers {
	public class DirectoryHelper {
		/// <summary>
		/// It will be default desktop folder + some GUID
		/// </summary>
		public static string GetDefaultDownloadFolderPath() {
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
				$"Anime_Download_{Guid.NewGuid()}");
		}
	}
}