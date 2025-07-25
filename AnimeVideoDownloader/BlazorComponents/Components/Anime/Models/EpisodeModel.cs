namespace BlazorComponents.Components.Anime.Models
{
    public class EpisodeModel
    {
        public int Number { get; set; }
        public string Title { get; set; } = "";
        public EpisodeStatus Status { get; set; }
    }

    public enum EpisodeStatus
    {
        Downloaded,
        InProgress,
        Error
    }
}
