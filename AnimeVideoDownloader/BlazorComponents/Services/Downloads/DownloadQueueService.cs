using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.Downloads;

public sealed class DownloadQueueService
{
    private readonly ILogger<DownloadQueueService> _logger;

    public ObservableCollection<QueueItem> Queue { get; } = new();

    public QueueItem? Current { get; private set; }

    public event Action? Changed;

    public DownloadQueueService(ILogger<DownloadQueueService> logger)
    {
        _logger = logger;
    }

    public void Enqueue(QueueItem item)
    {
        Queue.Add(item);
        Changed?.Invoke();
    }

    public void Remove(QueueItem item)
    {
        if (ReferenceEquals(item, Current))
        {
            Current = null;
        }
        Queue.Remove(item);
        Changed?.Invoke();
    }

    public void Clear()
    {
        Queue.Clear();
        Changed?.Invoke();
    }

    public void MoveUp(QueueItem item)
    {
        var idx = Queue.IndexOf(item);
        if (idx > 0)
        {
            Queue.Move(idx, idx - 1);
            Changed?.Invoke();
        }
    }

    public void MoveDown(QueueItem item)
    {
        var idx = Queue.IndexOf(item);
        if (idx >= 0 && idx < Queue.Count - 1)
        {
            Queue.Move(idx, idx + 1);
            Changed?.Invoke();
        }
    }

    public void StartNext()
    {
        if (Current is { Status: not QueueItemStatus.Downloading })
        {
            // do nothing if current is paused or queued; caller should manage state
        }
        if (Current is null && Queue.Any())
        {
            Current = Queue[0];
            Queue.RemoveAt(0);
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

public enum QueueItemStatus
{
    Queued,
    Downloading,
    Paused,
}

public sealed class QueueItem
{
    public required string AnimeTitle { get; set; }
    public required int EpisodeNumber { get; set; }
    public required string EpisodeTitle { get; set; }

    public double Progress { get; set; }
    public string? DownloadSpeed { get; set; }
    public string? ETA { get; set; }
    public string? TotalBytes { get; set; }

    public QueueItemStatus Status { get; set; } = QueueItemStatus.Queued;
}