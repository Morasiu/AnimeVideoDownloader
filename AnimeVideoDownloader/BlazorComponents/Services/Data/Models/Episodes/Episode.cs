using BlazorComponents.Services.Data.Models.Animes;

namespace BlazorComponents.Services.Data.Models.Episodes;

public class Episode
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public EpisodeStatus Status { get; set; }
    public string? PageUri { get; set; }
    
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