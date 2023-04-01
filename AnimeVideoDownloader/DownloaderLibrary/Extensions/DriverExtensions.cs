using OpenQA.Selenium.Remote;

namespace DownloaderLibrary.Extensions {
	public static class DriverExtensions {
		public static void RemoveElementById(this RemoteWebDriver driver, string id) {
			driver.ExecuteScript($"document.getElementById('{id}').remove();");
		}
		
		public static void RemoveElementByTagName(this RemoteWebDriver driver, string tag) {
			driver.ExecuteScript($"document.getElementsByTagName('{tag}')[0].remove();");
		}
		
		public static void RemoveElementByClassName(this RemoteWebDriver driver, string className) {
			driver.ExecuteScript($"document.getElementsByClassName('{className}')[0].remove();");
		}
	}
}