using System.Collections.ObjectModel;
using BlazorComponents.Extensions;
using BlazorComponents.Services.AnimeServices;
using BlazorComponents.Services.Data;
using BlazorComponents.Services.Data.Models.Episodes;
using BlazorComponents.Services.Data.Models.EpisodeSources;
using BlazorComponents.Services.Data.Models.QueueItems;
using BlazorComponents.Services.YoutubeDLService;
using Codeuctivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.Downloads;

public sealed class DownloadQueueService
{
    private readonly ApplicationDbContext _context;
    private readonly DownloaderService _downloaderService;
    private readonly IAnimeService _animeService;
    private readonly ILogger<DownloadQueueService> _logger;

    private ObservableCollection<QueueItem>? queue;

    // The queue itself contains the current item at index 0 (lowest Order).
    public ObservableCollection<QueueItem> Queue
    {
        get
        {
            EnsureQueueLoaded();
            return queue!;
        }
    }

    public event Action? Changed;
    public event Action<AnimeDownloadDownloadProgress>? DownloadProgressChanged;

    public DownloadQueueService(ApplicationDbContext context, DownloaderService downloaderService, IAnimeService animeService, ILogger<DownloadQueueService> logger)
    {
        _context = context;
        _downloaderService = downloaderService;
        _animeService = animeService;
        _logger = logger;
        StopItemInProgressAsync().GetAwaiter().GetResult();
    }

    public async Task EnqueueAsync(Episode episode)
    {
        var item = new QueueItem
        {
            Order = Queue.Select(x => x.Order).DefaultIfEmpty().Max() + 1,
            Episode = episode,
        };
        queue!.Add(item);
        await _context.SaveChangesAsync();
        Changed?.Invoke();
    }

    public async Task DequeueAsync(QueueItem item)
    {
        // Get the order of the item being removed
        var removedOrder = item.Order;

        // Remove the item from the queue
        item.Order = int.MaxValue;

        // Update the order of all items that come after the removed item
        var itemsToUpdate = Queue.Where(x => x.Order > removedOrder).ToList();
        foreach (var queueItem in itemsToUpdate)
        {
            queueItem.Order--;
        }
        await _context.SaveChangesAsync();
        Changed?.Invoke();
    }

    private async Task StartDownloadAsync(QueueItem item)
    {
        item.Status = QueueItemStatus.Downloading;
        if (item.Episode.Sources.Count == 0 && item.Episode.SourcesUpdatedAt is null)
        {
            item.Status = QueueItemStatus.LookingForSources;
            await _context.SaveChangesAsync();
            await _animeService.UpdateEpisodeSourcesAsync(item.Episode);
        }
        item.Status = QueueItemStatus.Downloading;
        await _context.SaveChangesAsync();
        foreach (var episodeSource in item.Episode.Sources
                     .Where(x => x.Status == EpisodeSourceStatus.Valid)
                     .OrderByDescending(x => x.Quality))
        {
            var (success, downloadedFilePath) = await DownloadEpisodeFromSourceAsync(item, episodeSource);
            if (success)
            {
                item.Status = QueueItemStatus.Completed;
                item.DownloadedAt = DateTime.UtcNow;
                item.Episode.Status = EpisodeStatus.Downloaded;
                var fullFilePath = RenameDownloadedFile(item, downloadedFilePath);
                item.Episode.FilePath = fullFilePath;
                await _context.SaveChangesAsync();
                await StartNextAsync();
                Changed?.Invoke();
                return;
            }
            episodeSource.Status = EpisodeSourceStatus.Error;
            await _context.SaveChangesAsync();
        }
        item.Episode.Status = EpisodeStatus.Error;
        _logger.LogError("Cannot download episode {EpisodeNumber} - {EpisodeTitle} from {EpisodeSourceUrl} from anime {AnimeTitle}", item.Episode.Number, item.Episode.Title, item.Episode.SourceUri, item.Episode.Anime.Title);
        await _context.SaveChangesAsync();
        await StartNextAsync();
        Changed?.Invoke();
    }

    private static string RenameDownloadedFile(QueueItem item, string? downloadedFilePath)
    {
        var fullFilePath = Path.Combine(item.Episode.Anime.Directory, $"{item.Episode.Number} - {item.Episode.Title.SanitizeFilename("_")}{Path.GetExtension(downloadedFilePath)}");
        File.Move(downloadedFilePath!, fullFilePath, true);
        return fullFilePath;
    }

    private async Task<(bool Success, string? DownloadedFilePath)> DownloadEpisodeFromSourceAsync(QueueItem item, EpisodeSource episodeSource)
    {
        var url = episodeSource.Url;
        var downloadPath = item.Episode.Anime.Directory;
        var progress = new Progress<AnimeDownloadDownloadProgress>(p =>
        {
            DownloadProgressChanged?.Invoke(p);
            if (p.Error is not null)
            {
                _logger.LogError("Error downloading episode {EpisodeNumber} from {EpisodeSourceUrl}. Error: {Error}", item.Episode.Number, url, p.Error);
            }
        });
        var downloadedFilePath = await _downloaderService.DownloadAsync(url, downloadPath, progress);
        if (downloadedFilePath.IsNullOrEmpty())
        {
            return (false, null);
        }
        return (true, downloadedFilePath);
    }

    public async Task RemoveAsync(QueueItem item)
    {
        Queue.Remove(item);
        await _context.SaveChangesAsync();
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        Queue.Clear();
        Changed?.Invoke();
        await _context.SaveChangesAsync();
    }

    public async Task MoveUpAsync(QueueItem item)
    {
        var itemBefore = Queue.FirstOrDefault(x => x.Order == item.Order - 1);
        if (itemBefore != null)
        {
            itemBefore.Order++;
        }
        item.Order--;
        await _context.SaveChangesAsync();
        Changed?.Invoke();
    }

    public async Task MoveDownAsync(QueueItem item)
    {
        var itemAfter = Queue.FirstOrDefault(x => x.Order == item.Order + 1);
        if (itemAfter != null)
        {
            itemAfter.Order--;
        }
        item.Order++;
        await _context.SaveChangesAsync();
        Changed?.Invoke();
    }

    public async Task StopItemInProgressAsync()
    {
        var itemInProgress = Queue.FirstOrDefault(x => x.Status is QueueItemStatus.Downloading or QueueItemStatus.LookingForSources);
        if (itemInProgress is null) return;
        itemInProgress.Status = QueueItemStatus.Queued;
        itemInProgress.Episode.Status = EpisodeStatus.New;
        await _context.SaveChangesAsync();
        Changed?.Invoke();
    }

    public async Task StartNextAsync()
    {
        while (true)
        {
            if (!Queue.Any()) return;
            var first = Queue.OrderBy(x => x.Order).First();
            if (first.Status == QueueItemStatus.Downloading) return;
            if (first.Status == QueueItemStatus.Completed) await DequeueAsync(first);
            var item = Queue.OrderBy(x => x.Order).FirstOrDefault(x => x.Status != QueueItemStatus.Downloading);
            if (item is not null)
            {
                await StartDownloadAsync(item);
                continue;
            }
            break;
        }
    }

    private void EnsureQueueLoaded()
    {
        if (queue is null)
        {
            _context.QueueItems.Load();
            queue = _context.QueueItems.Local.ToObservableCollection();
        }
    }
}