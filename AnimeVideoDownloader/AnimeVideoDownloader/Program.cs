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
			Console.Clear();
			Console.WriteLine($"=== Anime Video Downloader App - {Assembly.GetExecutingAssembly().GetName().Version} ===");
			Console.CursorVisible = false;
			var progress = new Progress<DownloadProgressData>(d => OnProgressChanged(null, d));
			progress.ProgressChanged += OnProgressChanged;

			if (args.Length < 2) {
				Console.WriteLine("Download URL and DownloadPath is needed.");
				return;
			}

			var episodesUrl = new Uri(args[0]);
			var downloadDirectory = args[1];
			using var manager = new DownloaderFactory(episodesUrl, downloadDirectory, progress);
			lock (ConsoleWriterLock) {
				_episodes = manager.GetEpisodes();
			}
			await manager.DownloadAllEpisodesAsync();
		}

		private static void OnProgressChanged(object sender, DownloadProgressData e) {
			lock (ConsoleWriterLock) {
				Console.SetCursorPosition(0, e.EpisodeNumber);
				var name = _episodes.Single(episode => episode.Number == e.EpisodeNumber).Name;
				var totalBytes = ByteSize.FromBytes(e.TotalBytes).ToString("0.00");
				var bytesReceived = ByteSize.FromBytes(e.BytesReceived).ToString("0.00");
				var bytesPerSecond = ByteSize.FromBytes(e.BytesPerSecond).ToString("0.00");
				var timeRemained = TimeSpan.FromSeconds((e.TotalBytes - e.BytesReceived) / (e.BytesPerSecond == 0 ? 1.0 : e.BytesPerSecond));
				if (e.Error != null) Console.ForegroundColor = ConsoleColor.Red;
				else if (Math.Abs(e.Percent - 1.0) < 0.01) Console.ForegroundColor = ConsoleColor.Green;
				else Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write($">> {name} {Math.Round(e.Percent * 100, 2)}% - {bytesReceived}/{totalBytes}" +
				              $" | {bytesPerSecond}/s Remained: {timeRemained:hh\\:mm\\:ss} " +
				              $"{(e.Error != null ? string.Concat("ERROR: ", e.Error) : string.Empty)}");
				Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
				Console.Write('\n');
				Console.ResetColor();
				Console.SetCursorPosition(0, e.EpisodeNumber);
			}
		}
	}
}