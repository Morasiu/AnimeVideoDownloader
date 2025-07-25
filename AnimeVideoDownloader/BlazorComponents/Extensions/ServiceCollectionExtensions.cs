using BlazorComponents.Components.Anime.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorComponents.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorComponentsServices(this IServiceCollection services)
        {
            // Register component services
            services.AddSingleton<IAnimeService, AnimeService>();

            return services;
        }
    }
}
