using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using DownloaderLibrary.Episodes;

namespace DownloaderLibrary.Checkpoints {
	public class JsonCheckpoint : ICheckpoint {
		private static readonly object Lock = new object();

		public IEnumerable<Episode> Load(string downloadDirectoryPath) {
			CheckDirectory(downloadDirectoryPath);

			var path = GetCheckpointPath(downloadDirectoryPath);
			var json = File.ReadAllText(path, Encoding.UTF8);
			var episodes = JsonSerializer.Deserialize<List<Episode>>(json);
			return episodes;
		}

		public void Save(string downloadDirectoryPath, IEnumerable<Episode> episodes) {
			CheckDirectory(downloadDirectoryPath);

			var jsonSerializerOptions = new JsonSerializerOptions {
				WriteIndented = true,
			};
			var json = JsonSerializer.Serialize(episodes, jsonSerializerOptions);
			var path = GetCheckpointPath(downloadDirectoryPath);
			lock (Lock) {
				File.WriteAllText(path, json, Encoding.UTF8);
			}
		}

		public bool Exist(string downloadDirectoryPath) {
			return File.Exists(Path.Combine(downloadDirectoryPath, "checkpoint.json"));
		}

		private static string GetCheckpointPath(string downloadDirectoryPath) {
			var path = Path.Combine(downloadDirectoryPath, "checkpoint.json");
			return path;
		}

		private static void CheckDirectory(string downloadDirectoryPath) {
			if (!Directory.Exists(downloadDirectoryPath))
				throw new ArgumentException($"Directory ({downloadDirectoryPath}) do not exist.",
					nameof(downloadDirectoryPath));
		}
	}
}