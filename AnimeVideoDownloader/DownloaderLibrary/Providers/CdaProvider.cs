using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DownloaderLibrary.Providers {
	public class CdaProvider : BaseProvider {
		public CdaProvider(RemoteWebDriver driver) : base(driver) {
		}

		public override Task<Uri> GetVideoSourceAsync(string url) {
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

			// Go to CDA page
			Driver.Url = url;

			var source =
				wait.Until(ExpectedConditions.ElementExists(By.TagName("video")));

			var sourceUrl = source.GetAttribute("src");
			return Task.FromResult(new Uri(sourceUrl));
		}
	}
}