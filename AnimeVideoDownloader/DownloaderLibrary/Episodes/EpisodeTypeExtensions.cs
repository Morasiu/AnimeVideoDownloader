using System;

namespace DownloaderLibrary.Episodes {
	public static class EpisodeTypeExtensions {
		public static string GetLetter(this EpisodeType type) {
			switch (type) {
				case EpisodeType.Normal:
					return " ";
				case EpisodeType.Filler:
					return "F";
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
	}
}