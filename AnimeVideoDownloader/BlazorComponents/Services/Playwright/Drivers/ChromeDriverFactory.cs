using Microsoft.Playwright;

namespace BlazorComponents.Services.Playwright.Drivers;

public class ChromeDriverFactory {
	public static async Task<IBrowser> CreateNewAsync()
	{
		var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
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