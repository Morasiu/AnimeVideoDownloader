using BlazorComponents.Components.Anime.Models;

namespace BlazorComponents.Components.Anime.Services
{
    public class AnimeService : IAnimeService
    {
        private readonly List<AnimeModel> _animeList =
        [
            new AnimeModel
            {
                Id = Guid.CreateVersion7(),
                Title = "Solo Leveling",
                Episodes =
                [
                    new() { Number = 1, Title = "Meeting Giyu", Status = EpisodeStatus.Downloaded },
                    new() { Number = 2, Title = "Final Selection", Status = EpisodeStatus.InProgress },
                    new() { Number = 3, Title = "Sabito & Makomo", Status = EpisodeStatus.Error },
                ]
            },

            new AnimeModel
            {
                Id = Guid.CreateVersion7(),
                Title = "Demon Slayer",
                Episodes =
                [
                    new() { Number = 1, Title = "Tanjiro's Journey", Status = EpisodeStatus.Downloaded },
                    new() { Number = 2, Title = "First Mission", Status = EpisodeStatus.InProgress },
                ]
            },

            new AnimeModel
            {
                Id = Guid.CreateVersion7(),
                Title = "Attack on Titan",
                Episodes =
                [
                    new() { Number = 1, Title = "The Final Battle", Status = EpisodeStatus.Downloaded },
                    new() { Number = 2, Title = "Freedom", Status = EpisodeStatus.Downloaded },
                ]
            },
        ];

        public List<AnimeModel> GetAnimeList()
        {
            return _animeList;
        }

        public Task<AnimeModel?> GetAnimeByIdAsync(Guid id)
        {
            return Task.FromResult(_animeList.FirstOrDefault(a => a.Id == id));
        }

        public async Task<AnimeModel> AddAnimeFromUrlAsync(string url, string directory, CancellationToken ct = default)
        {
            // Basic validation and parsing (placeholder logic)
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty", nameof(url));
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Directory cannot be empty", nameof(directory));

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) throw new ArgumentException("Invalid URL format", nameof(url));

            // Derive a simple title from the URL (host + last segment without dashes)
            
            // TODO Title

            // Generate new Id

            var anime = new AnimeModel
            {
                Id = Guid.CreateVersion7(),
                Title = string.Empty,
                SourceUrl = url,
                Directory = directory,
                Episodes = new List<EpisodeModel>()
            };

            _animeList.Add(anime);

            // Simulate async to keep signature async-friendly
            await Task.CompletedTask;
            return anime;
        }

        public bool HasDownloadingEpisodes(AnimeModel anime)
        {
            return anime.Episodes.Any(e => e.Status == EpisodeStatus.InProgress);
        }

        public bool HasErrorEpisodes(AnimeModel anime)
        {
            return anime.Episodes.Any(e => e.Status == EpisodeStatus.Error);
        }

        
    }
}
