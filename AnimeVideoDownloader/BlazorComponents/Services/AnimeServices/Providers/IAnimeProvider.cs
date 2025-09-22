using BlazorComponents.Services.Data.Models.Episodes;

namespace BlazorComponents.Services.AnimeServices.Providers;

public interface IAnimeProvider
{
    Task<string> GetAnimeTitleAsync(string sourceUri);
    
    Task<List<Episode>> GetEpisodesListAsync(string sourceUri);
}