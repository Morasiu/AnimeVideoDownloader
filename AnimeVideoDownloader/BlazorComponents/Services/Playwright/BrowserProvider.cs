using BlazorComponents.Services.Playwright.Drivers;
using Microsoft.Playwright;

namespace BlazorComponents.Services.Playwright;

public class BrowserProvider : IBrowserProvider
{
    private readonly Lazy<IBrowser> _browser = new(() => ChromeDriverFactory.CreateNewAsync().GetAwaiter().GetResult());

    public IBrowser GetBrowser()
    {
        return _browser.Value;
    }
}