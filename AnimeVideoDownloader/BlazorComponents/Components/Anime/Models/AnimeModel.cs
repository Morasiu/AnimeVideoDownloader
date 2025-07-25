namespace BlazorComponents.Components.Anime.Models
{
    public class AnimeModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Year { get; set; } = "";
        public string Tags { get; set; } = "";
        public string CurrentSeason { get; set; } = "";
        public List<EpisodeModel> Episodes { get; set; } = new();
    }
}
