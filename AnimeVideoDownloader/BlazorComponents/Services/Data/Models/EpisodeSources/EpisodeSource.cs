using System.Collections.ObjectModel;
using BlazorComponents.Services.Data.Models.Episodes;
using BlazorComponents.Services.Data.Models.QueueItems;

namespace BlazorComponents.Services.Data.Models.EpisodeSources;

public class EpisodeSource
{
    public Guid Id { get; set; }
    public required string Url { get; set; }
    public EpisodeSourceStatus Status { get; set; }
    public SourceKind Kind { get; set; }
    public Quality Quality { get; set; }
    public Language VoiceLanguage { get; set; }
    public Language SubtitlesLanguage { get; set; }
    
    public Guid EpisodeId { get; set; }
    public virtual Episode? Episode { get; set; }
    public virtual ICollection<QueueItem>? QueueItems { get; set; } = new ObservableCollection<QueueItem>();
}