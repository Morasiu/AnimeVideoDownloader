using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Downloader;

namespace DownloaderLibrary.Helpers {
	public static class DownloadPackageExtensions {
		private static readonly object FileLock = new object();

		public static void SavePackage(this DownloadPackage package, string episodePath) {
			var formattedPath = GetFilePath(episodePath);
			IFormatter formatter = new BinaryFormatter();
			var fileInfo = new FileInfo(formattedPath);
			if (!fileInfo.Exists) {
				fileInfo.Create().Dispose();
			}

			lock (FileLock) {
				Stream serializedStream = null;
				try {
					serializedStream = new FileStream(formattedPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
					formatter.Serialize(serializedStream, package);
				}
				finally {
					serializedStream?.Flush();
					serializedStream?.Close();
					serializedStream?.Dispose();
				}
			}
		}

		public static DownloadPackage LoadPackage(this DownloadPackage package, string episodePath) {
			var formattedPath = GetFilePath(episodePath);
			if (formattedPath == null) return null;
			IFormatter formatter = new BinaryFormatter();
			try {
				lock (FileLock) {
					Stream serializedStream = new FileStream(formattedPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

					var newPack = formatter.Deserialize(serializedStream) as DownloadPackage;
					serializedStream.Flush();
					serializedStream.Close();
					serializedStream.Dispose();
					return newPack;
				}
			}
			catch (FileNotFoundException) {
				return null;
			}
			catch (SerializationException) {
				return null;
			}
		}

		public static void Delete(this DownloadPackage package, string episodePath) {
			var formattedPath = GetFilePath(episodePath);
			if (!File.Exists(formattedPath)) return;
			File.Delete(formattedPath);
		}

		private static string GetFilePath(string episodePath) {
			if (episodePath == null) return null;
			var directoryName = Path.GetDirectoryName(episodePath);
			var fileName = Path.GetFileNameWithoutExtension(episodePath);
			var formattedPath = Path.Combine(directoryName ?? string.Empty, $"{fileName}.package");
			return formattedPath;
		}
	}
}