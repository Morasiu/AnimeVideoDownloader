using DownloaderLibrary.Data.Checkpoints;

namespace DownloaderLibrary.CheckpointManagers {
	public interface ICheckpoint {
		Checkpoint Load(string downloadDirectoryPath);

		void Save(string downloadDirectoryPath, Checkpoint checkpoint);

		bool Exist(string downloadDirectoryPath);
	}
}