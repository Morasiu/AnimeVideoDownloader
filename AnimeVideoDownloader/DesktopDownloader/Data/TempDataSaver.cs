using System.IO;
using Newtonsoft.Json;

namespace DesktopDownloader.Data {
	public static class TempDataSaver {
		private static readonly string TempFilePath;

		static TempDataSaver() {
			var tempPath = Path.GetTempPath();
			TempFilePath = Path.Combine(tempPath, "latestDownload.temp");
		}
		public static void Save(TempData data) {
			File.WriteAllText(TempFilePath, JsonConvert.SerializeObject(data));
		}

		public static TempData Load() {
			if (!File.Exists(TempFilePath)) {
				return null;
			}
			return JsonConvert.DeserializeObject<TempData>(File.ReadAllText(TempFilePath));
		}
	}
}