using System.Collections.ObjectModel;
using BlazorComponents.Services.Data.Models.Animes;
using BlazorComponents.Services.Data.Models.EpisodeSources;

namespace BlazorComponents.Services.Data.Models.Episodes;

public class Episode
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public EpisodeStatus Status { get; set; }
    public string? SourceUri { get; set; }
    public string? FilePath { get; set; }
    public long TotalBytes { get; set; }
    public EpisodeType EpisodeType { get; set; }
    public DateTime? SourcesUpdatedAt { get; set; }
    public virtual ICollection<EpisodeSource> Sources { get; set; } = new ObservableCollection<EpisodeSource>();
    
    public Guid AnimeId { get; set; }
    public virtual Anime Anime { get; set; } = null!;
}

public enum EpisodeStatus
{
    New, 
    InProgress,
    Downloaded,
    Error,
}