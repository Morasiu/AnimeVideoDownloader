namespace BlazorComponents.Components.Anime.Models
{
    public class AnimeModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<EpisodeModel> Episodes { get; set; } = new();
    }
}
