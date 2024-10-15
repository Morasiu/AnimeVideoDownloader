using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace DownloaderLibrary.Providers {
	public class GDriveProvider : BaseProvider {
		public GDriveProvider(WebDriver driver) : base(driver) { }

		public override async Task<Uri> GetVideoSourceAsync(string url) {
			var match = Regex.Match(url, @"\/d\/(.+)\/preview");
			var id = match.Groups[1].Value;
			if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Wrong Gdrive url: {url}");
			var uriBuilder = new UriBuilder() {
				Scheme = "https",
				Host = "drive.google.com",
				Path = "uc",
				Query = $"id={id}",
				Port = -1
			};

			var shortUri = uriBuilder.Uri;
			
			var downloadUriBuilder = new UriBuilder() {
				Scheme = "https",
				Host = "drive.google.com",
				Path = "uc",
				Query = $"id={id}&export=download",
				Port = -1
			};

			await CheckIf404(downloadUriBuilder.Uri);
			var downloadUri = downloadUriBuilder.Uri;
			return downloadUri;
		}

		private async Task CheckIf404(Uri uri) {
			var client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(3);
			client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/apng,*/*;q=0.8");
			client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
			client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
			client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9,en-US;q=0.8");
			client.DefaultRequestHeaders.Add("Connection", "keep-alive");
			client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
			client.DefaultRequestHeaders.Add("Pragma", "no-cache");
			client.DefaultRequestHeaders.UserAgent.Clear();
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko; Google Page Speed Insights) Chrome/27.0.1453 Safari/537.36");
			try {
				var response = await client.GetAsync(uri);
				if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.InternalServerError)
					throw new WebDriverTimeoutException("GDrive video not found.");
				if (response.Content.Headers.ContentType.MediaType == "text/html")
					throw new WebDriverTimeoutException($"GDrive video not found. {await response.Content.ReadAsStringAsync()}");
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