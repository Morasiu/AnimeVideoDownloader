using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DownloaderLibrary.Providers {
	public class GDriveProvider : BaseProvider {
		public GDriveProvider(RemoteWebDriver driver) : base(driver) { }

		public override async Task<Uri> GetVideoSourceAsync(string url) {
			var match = Regex.Match(url, @"\/d\/(.+)\/preview");
			var id = match.Groups[1].Value;
			if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Wrong Gdrive url: {url}");
			var uriBuilder = new UriBuilder() {
				Scheme = "https",
				Host = "drive.google.com",
				Path = "uc",
				Query = $"id={id}&export=download&confirm=t",
				Port = -1
			};

			var uri = uriBuilder.Uri;

			await CheckIf404(uri: uri);

			return uri;
		}

		private async Task CheckIf404(Uri uri) {
			var client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(1);
			try {
				var response = await client.GetAsync(uri);
				if (response.StatusCode == HttpStatusCode.NotFound)
					throw new WebDriverTimeoutException("GDrive video not found.");
			}
			catch (HttpRequestException) {
				throw new WebDriverTimeoutException("GDrive video not found.");
			}
			catch (TaskCanceledException) {
				throw new WebDriverTimeoutException("GDrive video not found.");
			}
			finally {
				client.Dispose();
			}
		}
	}
}