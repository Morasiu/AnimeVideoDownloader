using BlazorComponents.Services.Data.Models.Episodes;
using BlazorComponents.Services.Data.Models.EpisodeSources;

namespace BlazorComponents.Services.AnimeServices.Providers;

public interface IAnimeProvider
{
    Task<string> GetAnimeTitleAsync(string sourceUri);
    
    Task<List<Episode>> GetEpisodesListAsync(string sourceUri);
    Task<List<EpisodeSource>> GetEpisodeSourcesAsync(string episodeSourceUri);
}