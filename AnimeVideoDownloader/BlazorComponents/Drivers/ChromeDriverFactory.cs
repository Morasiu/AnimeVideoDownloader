using Microsoft.Playwright;

namespace BlazorComponents.Drivers {
	public class ChromeDriverFactory {
		public static async Task<IBrowser> CreateNewAsync()
		{
			var playwright = await Playwright.CreateAsync();
			var options = new BrowserTypeLaunchOptions()
			{
#if DEBUG
				Headless = false,
#endif
			};
			var browser = await playwright.Chromium.LaunchAsync(options);
			return browser;
		}
	}
}