using BlazorComponents.Services.AnimeServices.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorComponents.Services.AnimeServices;

public static class AnimeServicesServiceCollectionExtensions
{
    public static IServiceCollection AddAnimeServices(this IServiceCollection services)
    {
        services.AddSingleton<IAnimeService, AnimeService>();
        services.AddKeyedSingleton<IAnimeProvider, ShindenProvider>(AnimeProviderType.Shinden);
        return services;
    }
}