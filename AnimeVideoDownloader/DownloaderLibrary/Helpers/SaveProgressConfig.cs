using System;
using ByteSizeLib;

namespace DownloaderLibrary.Helpers {
	public class SaveProgressConfig {
		public const long SaveChunkBytes = ByteSize.BytesInMegaByte * 5;
		public static TimeSpan SaveTime = TimeSpan.FromSeconds(5);
	}
}