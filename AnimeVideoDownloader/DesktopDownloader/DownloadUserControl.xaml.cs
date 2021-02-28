using System;
using System.Collections.ObjectModel;
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

		public DownloadUserControl(BaseAnimeDownloader downloader) {
			InitializeComponent();
			Loaded += OnLoaded;
			_downloader = downloader;
			_downloader.ProgressChanged += OnProgress;
			Application.Current.MainWindow.Closed += (sender, args) => _downloader.Dispose();
			Application.Current.MainWindow.Closing += (sender, args) => _downloader.Dispose();
		}

		private void OnProgress(object sender, DownloadProgressData data) {
			
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
			// foreach (var episode in episodes) {
			// 	var wrapPanel = new WrapPanel();
			// 	var control = new EpisodeView();
			// 	control.Episode = episode;
			// 	// STATUS EMOJI
			// 	var status = new TextBlock();
			// 	status.Width = 20;
			// 	status.Text = "⌛";
			// 	control.Status = status;
			// 	wrapPanel.Children.Add(status);
			// 	
			// 	// EPISODE NUMBER
			// 	// EPISODE NAME
			// 	var episodeNumber = new TextBlock { Text = episode.Number.ToString() };
			// 	episodeNumber.Width = 20;
			// 	wrapPanel.Children.Add(episodeNumber);
			// 	
			// 	// EPISODE NAME
			// 	var episodeName = new TextBlock { Text = episode.Name };
			// 	episodeName.Width = 350;
			// 	wrapPanel.Children.Add(episodeName);
			//
			// 	// EPISODE TYPE
			// 	var episodeTypeText = new TextBlock { Text = episode.EpisodeType.GetLetter() };
			// 	episodeTypeText.Width = 20;
			// 	wrapPanel.Children.Add(episodeTypeText);
			//
			// 	// Percent
			// 	var percent = new TextBlock { Text = "0%" };
			// 	percent.Width = 40;
			// 	control.Percent = percent;
			// 	wrapPanel.Children.Add(percent);
			//
			// 	// Byte received
			// 	var bytesReceived = new TextBlock { Text = "000.00 MB" };
			// 	bytesReceived.Width = 65;
			// 	control.BytesReceived = bytesReceived;
			// 	wrapPanel.Children.Add(bytesReceived);
			//
			// 	// Slash
			// 	var slash = new TextBlock { Text = "/" };
			// 	slash.Width = 15;
			// 	wrapPanel.Children.Add(slash);
			//
			// 	// Total
			// 	var totalBytes = new TextBlock { Text = "000.00 MB" };
			// 	totalBytes.Width = 65;
			// 	control.TotalBytes = totalBytes;
			// 	wrapPanel.Children.Add(totalBytes);
			//
			// 	// Bytes per second
			// 	var bytesPerSecond = new TextBlock { Text = "000.00 MB/s" };
			// 	bytesPerSecond.Width = 80;
			// 	control.BytesPerSecond = bytesPerSecond;
			// 	wrapPanel.Children.Add(bytesPerSecond);
			//
			// 	// Bytes per second
			// 	var timeRemained = new TextBlock { Text = "Remained: 00:00:00" };
			// 	control.TimeRemained = timeRemained;
			// 	wrapPanel.Children.Add(timeRemained);
			//
			// 	// Error
			// 	var error = new TextBlock { Text = "", Foreground = new SolidColorBrush(Colors.Red) };
			// 	control.Error = error;
			// 	wrapPanel.Children.Add(error);
			//
			// 	if (episode.IsDownloaded && File.Exists(episode.Path)) {
			// 		status.Text = DoneEmoji;
			// 		percent.Text = "100%";
			// 		bytesReceived.Text = ByteSize.FromBytes(episode.TotalBytes).ToString("0.00");
			// 		totalBytes.Text = ByteSize.FromBytes(episode.TotalBytes).ToString("0.00");
			// 	}
			//
			// 	_controls.Add(episode.Number, control);
			// 	StackPanel.Children.Add(wrapPanel);
			// }

			DownloadAllButton.IsEnabled = true;
			LoadingBar.Visibility = Visibility.Collapsed;
		}

		private async void DownloadAllOnClick(object sender, RoutedEventArgs e) {
			LoadingBar.Visibility = Visibility.Visible;
			DownloadAllButton.IsEnabled = false;
			await DownloadAddEpisodesTaskAsync();
			LoadingBar.Visibility = Visibility.Collapsed;
		}

		private async Task DownloadAddEpisodesTaskAsync() {
			await _downloader.DownloadAllEpisodesAsync().ConfigureAwait(false);
		}
	}
}