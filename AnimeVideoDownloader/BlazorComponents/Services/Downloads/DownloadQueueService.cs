using System.Collections.ObjectModel;
using BlazorComponents.Services.Data;
using BlazorComponents.Services.Data.Models.QueueItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.Downloads;

public sealed class DownloadQueueService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DownloadQueueService> _logger;

    private ObservableCollection<QueueItem>? queue = null;

    public QueueItem? Current { get; private set; }

    public event Action? Changed;

    public DownloadQueueService(ApplicationDbContext context, ILogger<DownloadQueueService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ObservableCollection<QueueItem>> GetQueue()
    {
        await LoadQueueAsync();
        return queue!;
    }

    private async Task LoadQueueAsync()
    {
        if (queue is null)
        {
            await _context.QueueItems.LoadAsync();
            queue = _context.QueueItems.Local.ToObservableCollection();
        }
    }

    public async Task EnqueueAsync(QueueItem item)
    {
        await LoadQueueAsync();
        item.Order = queue!.Select(x => x.Order).DefaultIfEmpty().Max() + 1;
        queue!.Add(item);
        await _context.SaveChangesAsync();
        Changed?.Invoke();
    }

    public async Task RemoveAsync(QueueItem item)
    {
        if (ReferenceEquals(item, Current))
        {
            Current = null;
        }
        await LoadQueueAsync();
        queue!.Remove(item);
        await _context.SaveChangesAsync();
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        await LoadQueueAsync();
        queue!.Clear();
        Changed?.Invoke();
        await _context.SaveChangesAsync();
    }

    public async Task MoveUpAsync(QueueItem item)
    {
        await LoadQueueAsync();
        var itemBefore = queue!.FirstOrDefault(x => x.Order == item.Order - 1);
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
        await LoadQueueAsync();
        var itemAfter = queue!.FirstOrDefault(x => x.Order == item.Order + 1);
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
        if (Current is { Status: not QueueItemStatus.Downloading })
        {
            // do nothing if current is paused or queued; caller should manage state
        }
        await LoadQueueAsync();
        if (Current is null && queue!.Any())
        {
            Current = queue!.First();
            queue!.RemoveAt(0);
            Current.Status = QueueItemStatus.Downloading;
            Changed?.Invoke();
        }
    }

    public void SetCurrent(QueueItem? item)
    {
        Current = item;
        Changed?.Invoke();
    }

    public void Pause(QueueItem item)
    {
        item.Status = QueueItemStatus.Paused;
        Changed?.Invoke();
    }

    public void Resume(QueueItem item)
    {
        item.Status = QueueItemStatus.Queued; // will be picked up to start
        Changed?.Invoke();
    }

    public void UpdateProgress(double progressPercent, string? speed, string? eta, string? totalBytes)
    {
        if (Current is null) return;
        Current.Progress = progressPercent;
        Current.DownloadSpeed = speed;
        Current.ETA = eta;
        Current.TotalBytes = totalBytes;
        Current.Status = QueueItemStatus.Downloading;
        Changed?.Invoke();
    }
}