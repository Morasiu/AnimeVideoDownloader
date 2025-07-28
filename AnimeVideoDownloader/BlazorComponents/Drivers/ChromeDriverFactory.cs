using Microsoft.Playwright;

namespace BlazorComponents.Drivers {
	public class ChromeDriverFactory {
		public static async Task<IBrowser> CreateNewAsync()
		{
			int exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
			if (exitCode != 0)
			{
				throw new InvalidOperationException($"Failed to install Playwright with exit code {exitCode}.");
			}
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