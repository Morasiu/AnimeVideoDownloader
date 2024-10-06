using OpenQA.Selenium;

namespace DownloaderLibrary.Extensions {
	public static class DriverExtensions {
		public static void RemoveElementById(this WebDriver driver, string id) {
			driver.ExecuteScript($"document.getElementById('{id}').remove();");
		}
		
		public static void RemoveElementByTagName(this WebDriver driver, string tag) {
			driver.ExecuteScript($"document.getElementsByTagName('{tag}')[0].remove();");
		}
		
		public static void RemoveElementByClassName(this WebDriver driver, string className) {
			driver.ExecuteScript($"document.getElementsByClassName('{className}')[0].remove();");
		}
	}
}