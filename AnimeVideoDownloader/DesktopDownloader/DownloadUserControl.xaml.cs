using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ByteSizeLib;
using DesktopDownloader.Data;
using DownloaderLibrary;
using DownloaderLibrary.Downloaders;

namespace DesktopDownloader {
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class DownloadUserControl {
		public ObservableCollection<EpisodeView> EpisodeViews { get; set; } = new ObservableCollection<EpisodeView>();
		private readonly BaseAnimeDownloader _downloader;
		private DateTime lastUpdate = DateTime.Now;
		public DownloadUserControl(BaseAnimeDownloader downloader) {
			InitializeComponent();
			Loaded += OnLoaded;
			_downloader = downloader;
			_downloader.ProgressChanged += OnProgress;
			Application.Current.MainWindow.Closed += (sender, args) => _downloader.Dispose();
			Application.Current.MainWindow.Closing += (sender, args) => _downloader.Dispose();
		}

		private void OnProgress(object sender, DownloadProgressData data) {
			if (DateTime.Now - lastUpdate < TimeSpan.FromMilliseconds(100)) return;
			lastUpdate = DateTime.Now;
			var episodeView = EpisodeViews.Single(a => a.Episode.Number == data.EpisodeNumber);
			if (Math.Abs(data.Percent - 1) < 0.001) {
				episodeView.Status = Emoji.Done;
				episodeView.Percent = data.Percent;
				episodeView.BytesReceived = ByteSize.FromBytes(data.TotalBytes).ToString("0.00");
				episodeView.TotalBytes = ByteSize.FromBytes(data.TotalBytes).ToString("0.00");
				episodeView.BytesPerSecond = ByteSize.FromBytes(0).ToString("0.00") + "/s";
				return;
			}
			
			episodeView.Percent = data.Percent;
			episodeView.BytesReceived = ByteSize.FromBytes(data.BytesReceived).ToString("0.00");
			episodeView.TotalBytes = ByteSize.FromBytes(data.TotalBytes).ToString("0.00");
			episodeView.BytesPerSecond = ByteSize.FromBytes(data.BytesPerSecond).ToString("0.00") + "/s";
			episodeView.Error = data.Error;
			
			var timeRemained = TimeSpan.FromSeconds(
				(data.TotalBytes - data.BytesReceived) / (data.BytesPerSecond == 0 ? 1.0 : data.BytesPerSecond));
			episodeView.TimeRemained = $"Remained: {timeRemained:hh\\:mm\\:ss}";
		}

		private async void OnLoaded(object sender, RoutedEventArgs e) {
			LoadingBar.Visibility = Visibility.Visible;
			try {
				await _downloader.InitAsync();
			}
			catch (ChromeVersionException exception) {
				Console.WriteLine(exception);
				MessageBox.Show($"Please update your Chrome\n\n Full error:\n\n {exception.Message}",
					"Downloader", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				Application.Current.Dispatcher.InvokeShutdown();
				return;
			}

			var episodes = await _downloader.GetEpisodesAsync();
			EpisodeViews = new ObservableCollection<EpisodeView>(episodes.Select(a => new EpisodeView {Episode = a}));
			EpisodeListView.ItemsSource = EpisodeViews;
			DownloadAllButton.IsEnabled = true;
			LoadingBar.Visibility = Visibility.Collapsed;
		}

		private async void DownloadAllOnClick(object sender, RoutedEventArgs e) {
			LoadingBar.Visibility = Visibility.Visible;
			DownloadAllButton.IsEnabled = false;
			await  _downloader.DownloadAllEpisodesAsync();
			LoadingBar.Visibility = Visibility.Collapsed;
		}
	}
}