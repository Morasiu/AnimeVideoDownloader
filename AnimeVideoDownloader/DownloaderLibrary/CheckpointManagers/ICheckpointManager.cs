using System.Collections.Generic;
using DownloaderLibrary.Data.Checkpoints;
using DownloaderLibrary.Data.Episodes;

namespace DownloaderLibrary.CheckpointManagers {
	public interface ICheckpoint {
		Checkpoint Load(string downloadDirectoryPath);

		void Save(string downloadDirectoryPath, Checkpoint checkpoint);

		bool Exist(string downloadDirectoryPath);
	}
}