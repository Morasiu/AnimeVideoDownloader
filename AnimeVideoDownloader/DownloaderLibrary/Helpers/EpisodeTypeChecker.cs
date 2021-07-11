using DownloaderLibrary.Data.Episodes;

namespace DownloaderLibrary.Helpers {
	public class EpisodeTypeChecker {
		public static EpisodeType GetType(string type) {
			if (type.ToLower().Contains("filler")) {
				return EpisodeType.Filler;
			}
			else {
				return EpisodeType.Normal;
			}
		}
	}
}