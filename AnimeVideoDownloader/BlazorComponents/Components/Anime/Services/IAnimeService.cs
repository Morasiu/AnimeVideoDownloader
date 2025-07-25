using BlazorComponents.Components.Anime.Models;

namespace BlazorComponents.Components.Anime.Services
{
    public interface IAnimeService
    {
        List<AnimeModel> GetAnimeList();
        Task<AnimeModel?> GetAnimeByIdAsync(int id);
        bool HasDownloadingEpisodes(AnimeModel anime);
        bool HasErrorEpisodes(AnimeModel anime);
        string GetEpisodeStatusClass(EpisodeStatus status);
        string GetStatusText(EpisodeStatus status);
    }
}
