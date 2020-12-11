using System;

namespace DownloaderLibrary.Downloaders {
	public class ChromeVersionException : Exception {
		public ChromeVersionException(string message) : base(message) { }
	}
}