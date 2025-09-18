namespace BlazorComponents.Components.Anime.Models
{
    public class AnimeModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string? SourceUrl { get; set; }
        public string Directory { get; set; } = "";
        public List<EpisodeModel> Episodes { get; set; } = new();
    }
}
