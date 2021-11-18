using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DownloaderLibrary.Data.Episodes;

namespace DownloaderLibrary.Data.Checkpoints {
	public class Checkpoint {
		private string _episodeListUrl;
		private ObservableCollection<Episode> _episodes;

		public Checkpoint() {
			SetEpisodes(new List<Episode>());
		}
		
		public string EpisodeListUrl {
			get => _episodeListUrl;
			set {
				_episodeListUrl = value;
				UpdatedAt = DateTime.Now;
			}
		}

		public DateTime UpdatedAt { get; private set; }

		public ICollection<Episode> Episodes {
			get => _episodes;
			set => SetEpisodes(value);
		}

		private void SetEpisodes(IEnumerable<Episode> value) {
			_episodes = new ObservableCollection<Episode>(value);
			_episodes.CollectionChanged += (sender, args) => UpdatedAt = DateTime.Now;
		}
	}
}