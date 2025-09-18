using System.Collections.ObjectModel;
using BlazorComponents.Extensions;
using BlazorComponents.Services.Data;
using BlazorComponents.Services.Data.Models.Animes;
using BlazorComponents.Services.Data.Models.Episodes;
using BlazorComponents.Services.Playwright;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.AnimeService;

public class AnimeService : IAnimeService
{
    private readonly ApplicationDbContext _context;
    private readonly IBrowserProvider _browserProvider;
    private readonly ILogger<AnimeService> _logger;

    public AnimeService(ApplicationDbContext context, IBrowserProvider browserProvider, ILogger<AnimeService> logger)
    {
        _context = context;
        _browserProvider = browserProvider;
        _logger = logger;
    }

    public ObservableCollection<Anime> GetAnimeList()
    {
        return _context.Anime.Local.ToObservableCollection();
    }

    public async Task<Anime> AddAnimeFromUrlAsync(string url, string directory, string title = "", CancellationToken ct = default)
    {
        // Basic validation and parsing (placeholder logic)
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty", nameof(url));
        if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Directory cannot be empty", nameof(directory));
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) throw new ArgumentException("Invalid URL format", nameof(url));
        _logger.LogInformation("Adding anime from {Url}", url);
        if (title.IsNullOrEmpty())
        {
            title = await GetTitle(uri);
        }
        var anime = new Anime
        {
            Id = Guid.CreateVersion7(),
            Title = title,
            SourceUrl = url,
            Directory = directory,
            Episodes = new List<Episode>(),
            Status = AnimeStatus.Error,
        };
        _context.Anime.Add(anime);
        await _context.SaveChangesAsync(ct);
        return anime;
    }

    private async Task<string> GetTitle(Uri uri)
    {
        var browser = _browserProvider.GetBrowser();
        var page = await browser.NewPageAsync();
        await page.GotoAsync(uri.AbsoluteUri);
        var title = await page.Locator(".title").InnerTextAsync();
        return title;
    }

    public bool HasDownloadingEpisodes(Anime anime)
    {
        return anime.Episodes.Any(e => e.Status == EpisodeStatus.InProgress);
    }

    public bool HasErrorEpisodes(Anime anime)
    {
        return anime.Episodes.Any(e => e.Status == EpisodeStatus.Error);
    }
}