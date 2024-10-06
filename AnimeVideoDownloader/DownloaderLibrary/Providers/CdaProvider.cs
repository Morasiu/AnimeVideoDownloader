using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DownloaderLibrary.Providers {
	public class CdaProvider : BaseProvider {
		public CdaProvider(WebDriver driver) : base(driver) { }

		public override async Task<Uri> GetVideoSourceAsync(string url) {
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

			// Go to CDA page
			var episeodeId = url.Split('/').Last().Split('?').First();
			var cdaUrl = $"https://www.cda.pl/video/{episeodeId}";
			Driver.Url = cdaUrl;
			
			AcceptCookies();
			AcceptAdultWarning();
			ChangeQuality(wait);

			IWebElement video;
			try {
				video = wait.Until(ExpectedConditions.ElementExists(By.ClassName("pb-video-player")));
			}
			catch (WebDriverTimeoutException) {
				throw new WebDriverTimeoutException("Cannot load CDA player.");
			}
			var sourceUrl = video.GetAttribute("src");

			var uri = new Uri(sourceUrl);
			return uri;
		}

		private static void ChangeQuality(WebDriverWait wait) {
			IWebElement qualityList;
			try {
				var openButton = wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".pb-settings-click")));
				openButton.Click();
				qualityList =
					wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".pb-menu-slave.pb-menu-slave-indent")));
				var highestQuality = qualityList.FindElements(By.TagName("li")).Last();
				highestQuality.FindElement(By.TagName("a")).Click();
			}
			catch (WebDriverTimeoutException) {
				throw new WebDriverTimeoutException("Cannot load CDA player.");
			}
		}

		private void AcceptAdultWarning() {
			try {
				var adultWarningButton = new WebDriverWait(Driver, TimeSpan.FromSeconds(3)).Until(ExpectedConditions.ElementExists(By.XPath("//*[text()='Mam ukończone 18 lat lub zgodę opiekuna prawnego.']")));
				adultWarningButton.Click();
			}
			catch (WebDriverTimeoutException) {
				// ignored
			}
		}

		private void AcceptCookies() {
			// Accept cookies
			try {
				var acceptCookiesButton = new WebDriverWait(Driver, TimeSpan.FromSeconds(3)).Until(ExpectedConditions.ElementExists(By.XPath("//*[text()='Zgadzam się']")));
				acceptCookiesButton.Click();
			}
			catch (WebDriverTimeoutException) {
				// ignored
			}
		}
	}
}