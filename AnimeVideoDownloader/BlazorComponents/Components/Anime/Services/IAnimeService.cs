using BlazorComponents.Components.Anime.Models;

namespace BlazorComponents.Components.Anime.Services
{
    public interface IAnimeService
    {
        List<AnimeModel> GetAnimeList();
        Task<AnimeModel?> GetAnimeByIdAsync(Guid id);
        Task<AnimeModel> AddAnimeFromUrlAsync(string url, string directory, CancellationToken ct = default);
        bool HasDownloadingEpisodes(AnimeModel anime);
        bool HasErrorEpisodes(AnimeModel anime);
    }
}
