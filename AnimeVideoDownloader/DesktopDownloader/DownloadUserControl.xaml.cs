using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ByteSizeLib;
using DesktopDownloader.Data;
using DownloaderLibrary;
using DownloaderLibrary.Downloaders;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

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
				await Task.Run(() => _downloader.InitAsync());
			}
			catch (ChromeVersionException exception) {
				Console.WriteLine(exception);
				MessageBox.Show($"Please update your Chrome\n\n Full error:\n\n {exception.Message}",
					"Downloader", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				Application.Current.Dispatcher.InvokeShutdown();
				return;
			}

			var episodes = await _downloader.GetEpisodesAsync();
			EpisodeViews = new ObservableCollection<EpisodeView>(episodes.Select(a => new EpisodeView {
				Episode = a,
				IsIgnored = a.IsIgnored
			}));
			EpisodeListView.ItemsSource = EpisodeViews;
			DownloadAllButton.IsEnabled = true;
			LoadingBar.Visibility = Visibility.Collapsed;
		}

		private async void DownloadAllOnClick(object sender, RoutedEventArgs e) {
			LoadingBar.Visibility = Visibility.Visible;
			DownloadAllButton.IsEnabled = false;
			await Task.Run(() => _downloader.DownloadAllEpisodesAsync());
			LoadingBar.Visibility = Visibility.Collapsed;
		}

		private void IsIgnored_OnChecked(object sender, RoutedEventArgs e) {
			var checkbox = (CheckBox) sender;
			var episodeView = (EpisodeView) checkbox.DataContext;
			episodeView.IsIgnored = checkbox.IsChecked.Value;
			_downloader.UpdateEpisode(episodeView.Episode.Number, a => a.IsIgnored = checkbox.IsChecked.Value);
		}
		

		private void OnListViewIgnoredClick(object sender, RoutedEventArgs e) {
			var selectedItems = EpisodeListView.SelectedItems;
			foreach (var item in selectedItems) {
				var episodeView = (EpisodeView) item;
				episodeView.IsIgnored = true;
			}
		}

		private async void PauseButton_OnClick(object sender, RoutedEventArgs e) {
			var button = (Button) sender;
			// TODO HERE
			var episodeView = (EpisodeView) button.DataContext;
			if (episodeView.IsPaused) {
				episodeView.IsPaused = false;
				await Task.Run(() => _downloader.DownloadEpisode(episodeView.Episode));
			}
			else {
				episodeView.IsPaused = true;
				_downloader.CancelDownload(episodeView.Episode.Number);
			}
		}
	}
}