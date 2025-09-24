using System.Collections.ObjectModel;
using BlazorComponents.Extensions;
using BlazorComponents.Services.Data;
using BlazorComponents.Services.Data.Models.Episodes;
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

    public DownloadQueueService(ApplicationDbContext context, DownloaderService downloaderService, ILogger<DownloadQueueService> logger)
    {
        _context = context;
        _downloaderService = downloaderService;
        _logger = logger;
    }

    public async Task EnqueueAsync(QueueItem item)
    {
        item.Order = Queue.Select(x => x.Order).DefaultIfEmpty().Max() + 1;
        queue!.Add(item);
        if (Queue.Count == 1)
        {
            await StartDownloadAsync(item);
        }
        await _context.SaveChangesAsync();
        Changed?.Invoke();
    }

    private async Task StartDownloadAsync(QueueItem item)
    {
        item.Status = QueueItemStatus.Downloading;
        item.EpisodeSource!.Episode!.Status = EpisodeStatus.InProgress;
        var url = item.EpisodeSource!.Url;
        var downloadPath = item.EpisodeSource!.Episode!.Anime.Directory;
        var progress = new Progress<AnimeDownloadDownloadProgress>(p =>
        {
            DownloadProgressChanged?.Invoke(p);
        });
        var downloadedFilePath = await _downloaderService.DownloadAsync(url, downloadPath, progress);
        if (downloadedFilePath.IsNullOrEmpty())
        {
            item.Status = QueueItemStatus.Error;
            item.EpisodeSource.Episode.Status = EpisodeStatus.Error;
            await _context.SaveChangesAsync();
            return;
        }
        item.Status = QueueItemStatus.Completed;
        item.EpisodeSource.Episode.Status = EpisodeStatus.Downloaded;
        var fullFilePath = Path.Combine(downloadPath, $"{item.EpisodeSource.Episode.Number} - {item.EpisodeSource.Episode.Title.SanitizeFilename("_")}{Path.GetExtension(downloadedFilePath)}");
        File.Move(downloadedFilePath, fullFilePath, true);
        item.EpisodeSource.Episode.FilePath = fullFilePath;
        await _context.SaveChangesAsync();
        Changed?.Invoke();
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

    public async Task StartNextAsync()
    {
        if (!Queue.Any()) return;
        if (!Queue.Any(x => x.Status == QueueItemStatus.Downloading))
        {
            await StartDownloadAsync(Queue.First());
        }
    }

    private void EnsureQueueLoaded()
    {
        if (queue is null)
        {
            _context.QueueItems.Load();
            queue = new ObservableCollection<QueueItem>(_context.QueueItems.Local.OrderBy(x => x.Order));
        }
    }
}