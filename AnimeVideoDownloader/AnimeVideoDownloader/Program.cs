using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ByteSizeLib;
using DownloaderLibrary;
using DownloaderLibrary.Episodes;

namespace ConsoleApp {
	class Program {
		private static readonly object ConsoleWriterLock = new object();
		private static IEnumerable<Episode> _episodes;

		static async Task Main(string[] args) {
			Console.WriteLine($"=== Anime Video Downloader App - {Assembly.GetExecutingAssembly().GetName().Version} ===");
			Console.CursorVisible = false;
			using var manager = new DownloaderManager(new Uri(args[0]), args[1]);
			_episodes = manager.GetEpisodes();
			var progress = new Progress<DownloadProgress>(d => OnProgressChanged(null, d));
			progress.ProgressChanged += OnProgressChanged;
			await manager.DownloadAllEpisodes(progress);
		}

		private static void OnProgressChanged(object sender, DownloadProgress e) {
			lock (ConsoleWriterLock) {
				Console.SetCursorPosition(0, e.EpisodeNumber);
				var name = _episodes.Single(episode => episode.Number == e.EpisodeNumber).Name;
				var totalBytes = ByteSize.FromBytes(e.TotalBytes);
				var bytesReceived = ByteSize.FromBytes(e.BytesReceived);
				var bytesPerSecond = ByteSize.FromBytes(e.BytesPerSecond);
				Console.Write(
					$"<== {name} {Math.Round(e.Percent * 100, 2)}% - {bytesReceived}/{totalBytes} | {bytesPerSecond}/s {(e.Error != null ? string.Concat("ERROR: ", e.Error) : string.Empty)}");
				Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
				Console.SetCursorPosition(0, e.EpisodeNumber);
			}
		}
	}
}