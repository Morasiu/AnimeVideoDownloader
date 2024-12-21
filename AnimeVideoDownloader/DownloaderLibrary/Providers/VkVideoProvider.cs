using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Downloader;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DownloaderLibrary.Providers {
	public class VkVideoProvider : BaseProvider {
		public VkVideoProvider(WebDriver driver) : base(driver) { }

		public override Task<Uri> GetVideoSourceAsync(string url) {
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
			
			Driver.Url = url;
			wait.Until(ExpectedConditions.ElementExists(By.TagName("video")));
			wait.Until(ExpectedConditions.ElementExists(By.TagName("script")));
			var scripts = Driver.FindElements(By.TagName("script"));
			var vkScirpt = scripts.First(e => e.GetAttribute("innerHTML").Contains("if (window.devicePixelRatio >= 2)")).GetAttribute("innerHTML");
			var regex = new Regex("\"url\\d+\":\"([^\"]+)\"");
			var matches = regex.Matches(vkScirpt);
			var urls = matches.Cast<Match>().Select(x => x.Groups[1]).ToList();
			var bestUrl = urls.Last().Value;
			var cleanUrl = Regex.Unescape(bestUrl);
			var uri = new Uri(cleanUrl);
			return Task.FromResult(uri);
		}
	}
}