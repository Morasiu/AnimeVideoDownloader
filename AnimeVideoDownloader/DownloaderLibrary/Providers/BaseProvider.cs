using System;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace DownloaderLibrary.Providers {
	public abstract class BaseProvider {
		protected readonly WebDriver Driver;

		protected BaseProvider(WebDriver driver) {
			Driver = driver;
		}

		public abstract Task<Uri> GetVideoSourceAsync(string url);
	}
}