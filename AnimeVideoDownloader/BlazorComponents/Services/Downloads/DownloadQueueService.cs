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

    public DownloadQueueService(ApplicationDbContext context, ILogger<DownloadQueueService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    private void ResortQueueByOrder()
    {
        if (queue is null) return;
        var ordered = queue.OrderBy(x => x.Order).ToList();
        if (ordered.Count == queue.Count && ordered.SequenceEqual(queue))
        {
            return; // already sorted
        }
        queue.Clear();
        foreach (var item in ordered)
        {
            queue.Add(item);
        }
    }

    public async Task EnqueueAsync(QueueItem item)
    {
        item.Order = Queue.Select(x => x.Order).DefaultIfEmpty().Max() + 1;
        queue!.Add(item);
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
        ResortQueueByOrder();
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
        ResortQueueByOrder();
        Changed?.Invoke();
    }

    public async Task StartNextAsync()
    {
        if (!Queue.Any()) return;

        if (!Queue.Any(x => x.Status == QueueItemStatus.Downloading))
        {
            var first = Queue.First();
            first.Status = QueueItemStatus.Downloading;
            await _context.SaveChangesAsync();
            Changed?.Invoke();
        }
    }
    
    public void UpdateProgress(double progressPercent, string? speed, string? eta, string? totalBytes)
    {
        // Progress reporting for QueueItem is not modeled here; only status is tracked.
        if (Queue.Count == 0) return;
        var current = Queue.First();
        current.Status = QueueItemStatus.Downloading;
        Changed?.Invoke();
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