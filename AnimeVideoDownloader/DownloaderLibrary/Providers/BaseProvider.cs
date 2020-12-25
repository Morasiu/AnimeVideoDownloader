using System;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;

namespace DownloaderLibrary.Providers {
	public abstract class BaseProvider {
		protected readonly RemoteWebDriver Driver;

		protected BaseProvider(RemoteWebDriver driver) {
			Driver = driver;
		}

		public abstract Task<Uri> GetVideoSourceAsync(string url);
	}
}