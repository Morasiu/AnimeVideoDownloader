using System.Collections.ObjectModel;
using BlazorComponents.Services.Data.Models.Episodes;

namespace BlazorComponents.Services.Data.Models.Animes;

public class Anime
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string Directory { get; set; } = "";
    public AnimeStatus Status { get; set; } = AnimeStatus.Initializing;
    public DateTime? EpisodesUpdatedAt { get; set; }
    public virtual ICollection<Episode> Episodes { get; set; } = new ObservableCollection<Episode>();
}

public enum AnimeStatus
{
    Initializing,
    Error,
    Ready,
}