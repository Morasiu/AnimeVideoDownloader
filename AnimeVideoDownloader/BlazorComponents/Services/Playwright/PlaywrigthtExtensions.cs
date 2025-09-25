using Microsoft.Playwright;

namespace BlazorComponents.Services.Playwright;

public static class PlaywrightExtensions
{
    public static async Task<(string Path, Guid errorId)> TakeErrorScreenshotAsync(this IPage page)
    {
        var errorId = Guid.NewGuid();
        var screenshotPath = Path.Combine(Path.GetTempPath(), "AnimeDownloader", $"{errorId}.png");
        var pageScreenshotOptions = new PageScreenshotOptions
        {
            FullPage = true,
            Path = screenshotPath,
        };
        await page.ScreenshotAsync(pageScreenshotOptions);
        return (screenshotPath, errorId);
    }
}