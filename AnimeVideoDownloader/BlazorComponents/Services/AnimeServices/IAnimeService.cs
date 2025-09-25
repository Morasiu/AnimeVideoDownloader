using System.Collections.ObjectModel;
using BlazorComponents.Services.Data.Models.Animes;
using BlazorComponents.Services.Data.Models.Episodes;

namespace BlazorComponents.Services.AnimeServices
{
    public interface IAnimeService
    {
        Task<ObservableCollection<Anime>> GetAnimeListAsync();
        Task<Anime> AddAnimeFromUrlAsync(string url, string directory, string title = "", CancellationToken ct = default);
        Task DeleteAnimeAsync(Anime anime, CancellationToken ct = default);
        Task UpdateAnimeEpisodeListAsync(Anime anime);
        Task UpdateEpisodeSourcesAsync(Episode episode);
        bool HasDownloadingEpisodes(Anime anime);
        bool HasErrorEpisodes(Anime anime);
    }
}
