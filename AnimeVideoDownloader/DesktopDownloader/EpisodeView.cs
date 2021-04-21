using System.ComponentModel;
using System.Runtime.CompilerServices;
using ByteSizeLib;
using DesktopDownloader.Annotations;
using DesktopDownloader.Data;
using DownloaderLibrary.Episodes;

namespace DesktopDownloader {
	public class EpisodeView : INotifyPropertyChanged{
		private string _status = Emoji.InProgress;
		private double _percent = 0;
		private string _bytesReceived = ByteSize.FromBytes(0).ToString("0.00 MB");
		private string _totalBytes = ByteSize.FromBytes(0).ToString("0.00 MB");
		private string _bytesPerSecond = ByteSize.FromBytes(0).ToString("0.00") + "/s";
		private string _timeRemained = "Remained: 00:00:00" ;
		private string _error = null;
		private bool _isPaused = true;
		public Episode Episode { get; set; }

		public string Status {
			get => _status;
			set {
				if (value == _status) return;
				_status = value;
				OnPropertyChanged();
			}
		}

		public string EpisodeTypeLetter => Episode.EpisodeType.GetLetter();

		public double Percent {
			get => _percent;
			set {
				if (value.Equals(_percent)) return;
				_percent = value;
				OnPropertyChanged();
			}
		}

		public string BytesReceived {
			get => _bytesReceived;
			set {
				if (value == _bytesReceived) return;
				_bytesReceived = value;
				OnPropertyChanged();
			}
		}

		public string TotalBytes {
			get => _totalBytes;
			set {
				if (value == _totalBytes) return;
				_totalBytes = value;
				OnPropertyChanged();
			}
		}

		public string BytesPerSecond {
			get => _bytesPerSecond;
			set {
				if (value == _bytesPerSecond) return;
				_bytesPerSecond = value;
				OnPropertyChanged();
			}
		}

		public string TimeRemained {
			get => _timeRemained;
			set {
				if (value == _timeRemained) return;
				_timeRemained = value;
				OnPropertyChanged();
			}
		}

		public string Error {
			get => _error;
			set {
				if (value == _error) return;
				_error = value;
				OnPropertyChanged();
			}
		}

		public bool IsIgnored {
			get => Episode.IsIgnored;
			set {
				if (value == Episode.IsIgnored) return;
				Episode.IsIgnored = value;
				OnPropertyChanged();
			}
		}

		public bool IsPaused {
			get => _isPaused;
			set {
				if (value == _isPaused) return;
				_isPaused = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}