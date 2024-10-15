using System;
using System.Drawing;
using System.Threading.Tasks;
using DownloaderLibrary.Downloaders;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace DownloaderLibrary.Drivers {
	public class ChromeDriverFactory {
		public static async Task<ChromeDriver> CreateNewAsync() {
			var driverPath = new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
			var service = ChromeDriverService.CreateDefaultService();
			service.SuppressInitialDiagnosticInformation = true;
			service.HideCommandPromptWindow = true;

			var chromeOptions = new ChromeOptions();
			chromeOptions.AddUserProfilePreference("intl.accept_languages", "pl-PL,pl");
			chromeOptions.AddArgument("no-sandbox");
			chromeOptions.AddArgument("disable-dev-shm-usage");
#if !DEBUG
			// chromeOptions.AddArgument("headless");
#endif
			try {
				return await Task.Run(() => {
					var driver = new ChromeDriver(service, chromeOptions);
					driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
					driver.Manage().Window.Size = new Size(1920, 1080);
					return driver;
				}).ConfigureAwait(false);
			}
			catch (InvalidOperationException e) {
				throw new ChromeVersionException(e.Message);
			}
		}
	}
}