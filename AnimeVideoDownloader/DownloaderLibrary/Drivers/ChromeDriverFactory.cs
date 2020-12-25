using System;
using System.Threading.Tasks;
using DownloaderLibrary.Downloaders;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace DownloaderLibrary.Drivers {
	public class ChromeDriverFactory {
		public static async Task<ChromeDriver> CreateNewAsync() {
			new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
			var service = ChromeDriverService.CreateDefaultService();
			service.SuppressInitialDiagnosticInformation = true;
			service.HideCommandPromptWindow = true;

			var chromeOptions = new ChromeOptions();
#if !DEBUG
			chromeOptions.AddArgument("headless");
#endif
			try {
				return await Task.Run(() => new ChromeDriver(service, chromeOptions)).ConfigureAwait(false);
			}
			catch (InvalidOperationException e) {
				throw new ChromeVersionException(e.Message);
			}
		}
	}
}