using BlazorComponents.Components.Anime.Models;

namespace BlazorComponents.Components.Anime.Services
{
    public interface IAnimeService
    {
        List<AnimeModel> GetAnimeList();
        Task<AnimeModel?> GetAnimeByIdAsync(int id);
        Task<AnimeModel> AddAnimeFromUrlAsync(string url, CancellationToken ct = default);
        bool HasDownloadingEpisodes(AnimeModel anime);
        bool HasErrorEpisodes(AnimeModel anime);
        string GetEpisodeStatusClass(EpisodeStatus status);
        string GetStatusText(EpisodeStatus status);
    }
}
