using System;
using System.Net.Sockets;

namespace DownloaderLibrary.Helpers {
	public class DownloadExceptionHelper {
		public static bool IsServerError(Exception e) {
			return IsServerReturningForbidden(e) || IsConnectionTimeout(e) || IsServerReturningNotFound(e);
		}
		
		private static bool IsConnectionTimeout(Exception e) {
			return (e.GetBaseException() as SocketException)?.SocketErrorCode == SocketError.TimedOut;
		}

		private static bool IsServerReturningForbidden(Exception e) {
			return e.Message == "The remote server returned an error: (403) Forbidden.";
		}

		private static bool IsServerReturningNotFound(Exception e) {
			return e.Message.Contains("404");
		}
	}
}