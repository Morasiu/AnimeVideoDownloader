using BlazorComponents.Services.Data.Models.Episodes;
using BlazorComponents.Services.Playwright;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace BlazorComponents.Services.AnimeServices.Providers;

public sealed class ShindenProvider : IAnimeProvider
{
    private readonly IBrowserProvider _browserProvider;
    private readonly ILogger<ShindenProvider> _logger;

    public ShindenProvider(IBrowserProvider browserProvider, ILogger<ShindenProvider> logger)
    {
        _browserProvider = browserProvider;
        _logger = logger;
    }
    
    public async Task<string> GetAnimeTitleAsync(string sourceUri)
    {
        var page = await GetPageAsync(sourceUri);
        _logger.LogInformation("Getting anime title for {SourceUri}", sourceUri);
        var title = await page.Locator(".title").InnerTextAsync();
        _logger.LogInformation("Anime title for {SourceUri} retrieved: {Title}", sourceUri, title);
        await page.CloseAsync();
        return title;    
    }

    private async Task<IPage> GetPageAsync(string sourceUri)
    {
        _logger.LogInformation("Getting page {SourceUri}", sourceUri);
        var browser = _browserProvider.GetBrowser();
        var page = await browser.NewPageAsync();
        await page.GotoAsync(sourceUri);
        _logger.LogInformation("Page {SourceUri} loaded", sourceUri);
        return page;
    }

    public async Task<List<Episode>> GetEpisodesListAsync(string sourceUri)
    {
        var page = await GetPageAsync(sourceUri);
        var episodes = new List<Episode>();
        
        return episodes;
    }
}