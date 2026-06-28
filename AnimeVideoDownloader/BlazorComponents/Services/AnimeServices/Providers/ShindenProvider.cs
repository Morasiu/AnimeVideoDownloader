using BlazorComponents.Services.Data.Models.Episodes;
using BlazorComponents.Services.Data.Models.EpisodeSources;
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
                SourceUri = fullLink,
            };
            episodes.Add(episode);
        }
        return episodes;
    }

    public async Task<List<EpisodeSource>> GetEpisodeSourcesAsync(string episodeSourceUri)
    {
        var page = await GetPageAsync(episodeSourceUri);
        var sources = new List<EpisodeSource>();
        try
        {
            await RemoveFuckingAdsAsync(page);
        }
        catch (Exception e)
        {
            var (screenshotPath, errorId) = await page.TakeErrorScreenshotAsync();
            _logger.LogWarning(e, "Cannot close fucking ads on shinden. ErrorId: {ErrorId}. Saved screenshot to: {Path}", errorId, screenshotPath);
        }
        try
        {
            await AcceptFuckingCookiesAsync(page);
        }
        catch (Exception e)
        {
            var (screenshotPath, errorId) = await page.TakeErrorScreenshotAsync();
            _logger.LogWarning(e, "Cannot accept fucking cookies on shinden. ErrorId: {ErrorId}. Saved screenshot to: {Path}", errorId, screenshotPath);
        }

        var episodeRows = await page.Locator("section.box.episode-player-list > div.table-responsive > table > tbody").Locator("tr").AllAsync();
        foreach (var episodeRow in episodeRows)
        {
            var cells = await episodeRow.Locator("td").AllAsync();
            var sourceKind = await cells[0].InnerTextAsync();
            var quality = await cells[1].InnerTextAsync();
            var voiceLanguage = await cells[2].InnerTextAsync();
            var subtitlesLanguage = await cells[3].InnerTextAsync();
            await cells[5].Locator("a").DispatchEventAsync("click");
            string? sourceUrl;
            try
            {
                sourceUrl = await page.Locator(".player-container > iframe").GetAttributeAsync("src");
            }
            catch (Exception e)
            {
                var (screenshotPath, errorId) = await page.TakeErrorScreenshotAsync();
                _logger.LogError(e, "Cannot get source url for {EpisodeSourceUri}. SourceKind: {SourceKind}. ErrorId; {ErrorId}. Saved screenshot to {Path}", episodeSourceUri, sourceKind, errorId, screenshotPath);
                continue;
            }
            var source = new EpisodeSource
            {
                Kind = SourceKindParser.Parse(sourceKind),
                Quality = QualityParser.FromString(quality),
                VoiceLanguage = LanguageParser.Parse(voiceLanguage),
                SubtitlesLanguage = LanguageParser.Parse(subtitlesLanguage),
                Url = sourceUrl ?? "",
            };
            sources.Add(source);
        }
        await page.CloseAsync();
        return sources;
    }

    private static async Task AcceptFuckingCookiesAsync(IPage page)
    {
        var options = new LocatorClickOptions { Timeout = 1000 };
        await page.GetByText("Zaakceptuj wszystko").ClickAsync();
        await page.GetByText("Akceptuję").ClickAsync(options);
    }

    private static async Task RemoveFuckingAdsAsync(IPage page)
    {
        await page.Locator("html > iframe:last-child").ClickAsync(new LocatorClickOptions { Timeout = 1000 });
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
}