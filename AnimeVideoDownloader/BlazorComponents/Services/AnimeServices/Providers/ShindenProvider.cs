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
        var options = new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
        };
        var page = await browser.NewPageAsync(options);
        await page.GotoAsync(sourceUri);
        _logger.LogInformation("Page {SourceUri} loaded", sourceUri);
        return page;
    }

    public async Task<List<Episode>> GetEpisodesListAsync(string sourceUri)
    {
        var page = await GetPageAsync(sourceUri);
        var episodes = new List<Episode>();
        var table = page.Locator("table").Locator("tbody");
        var rows = await table.Locator("tr").AllAsync();
        foreach (var row in rows)
        {
            var link = await row.Locator("a").GetAttributeAsync("href");
            var fullLink = "https://shinden.pl/" + link;
            var number = await row.Locator("td:nth-child(1)").InnerTextAsync();
            var name = await row.Locator("td:nth-child(2)").InnerTextAsync();
            var episode = new Episode
            {
                Number = int.Parse(number),
                Status = EpisodeStatus.New,
                Title = name,
                PageUri = fullLink,
            };
            episodes.Add(episode);
        }
        
        return episodes;
    }
}