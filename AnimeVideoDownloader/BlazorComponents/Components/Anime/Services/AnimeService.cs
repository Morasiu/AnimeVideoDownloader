using BlazorComponents.Components.Anime.Models;

namespace BlazorComponents.Components.Anime.Services
{
    public class AnimeService : IAnimeService
    {
        private readonly List<AnimeModel> _animeList = new()
        {
            new AnimeModel
            {
                Id = 1,
                Title = "Solo Leveling",
                Episodes = new List<EpisodeModel>
                {
                    new() { Number = 1, Title = "Meeting Giyu", Status = EpisodeStatus.Downloaded },
                    new() { Number = 2, Title = "Final Selection", Status = EpisodeStatus.InProgress },
                    new() { Number = 3, Title = "Sabito & Makomo", Status = EpisodeStatus.Error }
                }
            },
            new AnimeModel
            {
                Id = 2,
                Title = "Demon Slayer",
                Episodes = new List<EpisodeModel>
                {
                    new() { Number = 1, Title = "Tanjiro's Journey", Status = EpisodeStatus.Downloaded },
                    new() { Number = 2, Title = "First Mission", Status = EpisodeStatus.InProgress }
                }
            },
            new AnimeModel
            {
                Id = 3,
                Title = "Attack on Titan",
                Episodes = new List<EpisodeModel>
                {
                    new() { Number = 1, Title = "The Final Battle", Status = EpisodeStatus.Downloaded },
                    new() { Number = 2, Title = "Freedom", Status = EpisodeStatus.Downloaded }
                }
            }
        };

        public List<AnimeModel> GetAnimeList()
        {
            return _animeList;
        }

        public Task<AnimeModel?> GetAnimeByIdAsync(int id)
        {
            return Task.FromResult(_animeList.FirstOrDefault(a => a.Id == id));
        }

        public bool HasDownloadingEpisodes(AnimeModel anime)
        {
            return anime.Episodes.Any(e => e.Status == EpisodeStatus.InProgress);
        }

        public bool HasErrorEpisodes(AnimeModel anime)
        {
            return anime.Episodes.Any(e => e.Status == EpisodeStatus.Error);
        }

        public string GetEpisodeStatusClass(EpisodeStatus status)
        {
            return status switch
            {
                EpisodeStatus.Downloaded => "downloaded",
                EpisodeStatus.InProgress => "in-progress",
                EpisodeStatus.Error => "error",
                _ => ""
            };
        }

        public string GetStatusText(EpisodeStatus status)
        {
            return status switch
            {
                EpisodeStatus.Downloaded => "Downloaded",
                EpisodeStatus.InProgress => "In Progress",
                EpisodeStatus.Error => "Error",
                _ => ""
            };
        }
    }
}
