using BlazorComponents.Services.Data.Models.Episodes;

namespace BlazorComponents.Services.Data.Models.QueueItems;

public class QueueItem
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public QueueItemStatus Status { get; set; } = QueueItemStatus.Queued;
    public DateTime DownloadedAt { get; set; }
    public Guid EpisodeId { get; set; }
    public virtual Episode Episode { get; set; } = null!;
}

public enum QueueItemStatus
{
    Queued = 0,
    Downloading = 1,
    Error = 2,
    Completed = 3,
    LookingForSources = 4,
}