using System.Collections.ObjectModel;
using BlazorComponents.Extensions;
using BlazorComponents.Services.AnimeServices.Providers;
using BlazorComponents.Services.Data;
using BlazorComponents.Services.Data.Models.Animes;
using BlazorComponents.Services.Data.Models.Episodes;
using BlazorComponents.Services.Data.Models.EpisodeSources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.AnimeServices;

public class AnimeService : IAnimeService
{
    private readonly ApplicationDbContext _context;
    private readonly IAnimeProvider _animeProvider;
    private readonly ILogger<AnimeService> _logger;

    public AnimeService(ApplicationDbContext context, [FromKeyedServices(AnimeProviderType.Shinden)] IAnimeProvider animeProvider, ILogger<AnimeService> logger)
    {
        _context = context;
        _animeProvider = animeProvider;
        _logger = logger;
    }

    public async Task<ObservableCollection<Anime>> GetAnimeListAsync()
    {
        if (_context.Anime.Local.Count == 0)
        {
            await _context.Anime.LoadAsync();
        }
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
            title = await _animeProvider.GetAnimeTitleAsync(uri.AbsoluteUri);
        }
        var anime = new Anime
        {
            Id = Guid.CreateVersion7(),
            Title = title,
            SourceUrl = url,
            Directory = directory,
            Episodes = [],
            Status = AnimeStatus.Error,
        };
        _context.Anime.Add(anime);
        await _context.SaveChangesAsync(ct);
        await UpdateAnimeEpisodeListAsync(anime);
        return anime;
    }

    public async Task DeleteAnimeAsync(Anime anime, CancellationToken ct = default)
    {
        if (anime is null) throw new ArgumentNullException(nameof(anime));
        _logger.LogInformation("Deleting anime {AnimeId} - {Title}", anime.Id, anime.Title);

        // Remove related episodes first to avoid FK issues if cascade is not configured
        var episodes = _context.Set<Episode>().Where(e => e.AnimeId == anime.Id);
        _context.RemoveRange(episodes);

        // Attach if needed and remove anime
        if (_context.Entry(anime).State == EntityState.Detached)
        {
            _context.Attach(anime);
        }
        _context.Remove(anime);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted anime {AnimeId}", anime.Id);
    }

    public async Task UpdateAnimeEpisodeListAsync(Anime anime)
    {
        var episodes = await _animeProvider.GetEpisodesListAsync(anime.SourceUrl);
        foreach (var episode in episodes)
        {
            if (anime.Episodes.Any(e => e.Number == episode.Number)) continue;
            anime.Episodes.Add(episode);
        }
        anime.EpisodesUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateEpisodeSourcesAsync(Episode episode)
    {
        ArgumentNullException.ThrowIfNull(episode.SourceUri);
        List<EpisodeSource> sources = await _animeProvider.GetEpisodeSourcesAsync(episode.SourceUri);
        foreach (var source in sources)
        {
            if (episode.Sources.Any(e => e.Url == source.Url)) continue;
            episode.Sources.Add(source);
        }
        episode.SourcesUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public bool HasDownloadingEpisodes(Anime anime)
    {
        return anime.Episodes.Any(e => e.QueueItem is not null);
    }

    public bool HasErrorEpisodes(Anime anime)
    {
        return anime.Episodes.Any(e => e.Status == EpisodeStatus.Error);
    }
}