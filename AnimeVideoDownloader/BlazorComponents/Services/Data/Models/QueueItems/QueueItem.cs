using BlazorComponents.Services.Data.Models.EpisodeSources;

namespace BlazorComponents.Services.Data.Models.QueueItems;

public class QueueItem
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public QueueItemStatus Status { get; set; } = QueueItemStatus.Queued;
    public DateTime DownloadedAt { get; set; }
    public Guid EpisodeSourceId { get; set; }
    public virtual EpisodeSource? EpisodeSource { get; set; }
}

public enum QueueItemStatus
{
    Queued,
    Downloading,
    Paused,
    Error,
    Completed,
}