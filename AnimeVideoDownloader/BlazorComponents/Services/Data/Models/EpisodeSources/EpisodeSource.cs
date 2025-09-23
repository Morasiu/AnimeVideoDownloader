using BlazorComponents.Services.Data.Models.Episodes;

namespace BlazorComponents.Services.Data.Models.EpisodeSources;

public class EpisodeSource
{
    public Guid Id { get; set; }
    public required string Url { get; set; }
    public EpisodeSourceStatus Status { get; set; }
    public SourceKind Kind { get; set; }
    
    public Guid EpisodeId { get; set; }
    public virtual Episode? Episode { get; set; }
}