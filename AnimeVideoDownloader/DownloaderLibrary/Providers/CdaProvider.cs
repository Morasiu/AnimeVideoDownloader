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
			if (source.GetAttribute("class") == "pb-ad-video-player") {
				var adClick = Driver.FindElement(By.ClassName("pb-vid-click"));
				adClick.Click();
				var adTimeText = Driver.FindElement(By.ClassName("pb-max-time")).Text;
				var adTime = TimeSpan.Parse(adTimeText).Add(TimeSpan.FromSeconds(10));
				wait.Timeout = adTime;
				source = wait.Until(ExpectedConditions.ElementExists(By.ClassName("pb-video-player")));
			}

			var sourceUrl = source.GetAttribute("src");
			return Task.FromResult(new Uri(sourceUrl));
		}
	}
}