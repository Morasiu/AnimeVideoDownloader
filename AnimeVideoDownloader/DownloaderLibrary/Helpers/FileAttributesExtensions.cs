using System.IO;

namespace DownloaderLibrary.Helpers {
	public static class FileAttributesExtensions {
		public static FileAttributes AddAttribute(this FileAttributes attributes, FileAttributes attributesToAdd) {
			if ((attributes & attributesToAdd) == attributesToAdd) return attributes;
			return attributes | attributesToAdd;
		}
		
		public static FileAttributes RemoveAttribute(this FileAttributes attributes, FileAttributes attributesToRemove) {
			return attributes & ~attributesToRemove;
		}
	}
}