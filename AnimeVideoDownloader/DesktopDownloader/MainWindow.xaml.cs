using System;
using System.Diagnostics;
using System.IO.Packaging;
using System.Reflection;
using System.Windows;
using DesktopDownloader.Data;
using DownloaderLibrary;
using DownloaderLibrary.CheckpointManagers;
using DownloaderLibrary.Downloaders;
using DownloaderLibrary.Drivers;

namespace DesktopDownloader {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public MainWindow() {
			InitializeComponent();
			EpisodeUrlTextBox.Text = "https://rezero.wbijam.pl/pierwsza_seria.html";
			DownloadDirectoryTextBox.Text = "D:/ReZero";
			Loaded += OnLoaded;
			this.Title = $"Desktop Downloader {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}";
		}

		private void CheckEpisodesButton_OnClick(object sender, RoutedEventArgs e) {
			LoadingBar.Visibility = Visibility.Visible;
			var episodeUrl = EpisodeUrlTextBox.Text;
			if (!Uri.TryCreate(episodeUrl, UriKind.Absolute, out var uri)) {
				MessageBox.Show("Episode link is invalid.", "Error", MessageBoxButton.OK);
				return;
			}

			var downloadDirectory = DownloadDirectoryTextBox.Text;
			if (string.IsNullOrWhiteSpace(downloadDirectory)) {
				MessageBox.Show("Download directory is invalid.", "Error", MessageBoxButton.OK);
				return;
			}

			var tempData = new TempData {
				LatestDownloadPath = DownloadDirectoryTextBox.Text,
				LatestDownloadUri = EpisodeUrlTextBox.Text
			};

			TempDataSaver.Save(tempData);

			var factory = new DownloaderFactory();
			var downloaderConfig = new DownloaderConfig {
				ShouldDownloadFillers = DownloadFillersCheckbox.IsChecked.Value,
				CheckpointManager = new JsonCheckpoint(),
				DownloadDirectory = downloadDirectory,
			};
			var downloader = factory.GetDownloaderForSite(uri, downloaderConfig);

			if (downloader == null) {
				MessageBox.Show($"Site:\n {uri} not supported yet.", "Site not supported", MessageBoxButton.OK,
					MessageBoxImage.Error);
				return;
			}
			LoadingBar.Visibility = Visibility.Collapsed;
			this.Content = new DownloadUserControl(downloader);
		}

		private void DownloadDirectoryOpenDialogButton_OnClick(object sender, RoutedEventArgs e) {
			var a = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
			if (a.ShowDialog() == true) {
				DownloadDirectoryTextBox.Text = a.SelectedPath;
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			var tempData = TempDataSaver.Load();
			if (tempData is null) {
				tempData = new TempData {
					LatestDownloadPath = DownloadDirectoryTextBox.Text,
					LatestDownloadUri = EpisodeUrlTextBox.Text
				};
				TempDataSaver.Save(tempData);
			}

			DownloadDirectoryTextBox.Text = tempData.LatestDownloadPath;
			EpisodeUrlTextBox.Text = tempData.LatestDownloadUri;
		}
	}
}