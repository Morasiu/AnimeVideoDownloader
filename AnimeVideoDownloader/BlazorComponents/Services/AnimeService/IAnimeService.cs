using System.Collections.ObjectModel;
using BlazorComponents.Services.Data.Models.Animes;

namespace BlazorComponents.Services.AnimeService
{
    public interface IAnimeService
    {
        ObservableCollection<Anime> GetAnimeList();
        Task<Anime> AddAnimeFromUrlAsync(string url, string directory, string title = "", CancellationToken ct = default);
        bool HasDownloadingEpisodes(Anime anime);
        bool HasErrorEpisodes(Anime anime);
    }
}
