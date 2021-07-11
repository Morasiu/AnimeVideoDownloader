using System.Collections.Generic;
using DownloaderLibrary.Data.Episodes;

namespace DownloaderLibrary.Checkpoints {
	public interface ICheckpoint {
		IEnumerable<Episode> Load(string downloadDirectoryPath);

		void Save(string downloadDirectoryPath, IEnumerable<Episode> episodes);

		bool Exist(string downloadDirectoryPath);
	}
}