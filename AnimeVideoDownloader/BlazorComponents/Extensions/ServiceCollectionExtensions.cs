using BlazorComponents.Components.Anime.Services;
using BlazorComponents.Services;
using BlazorComponents.Services.AppData;
using BlazorComponents.Services.Initializers;
using BlazorComponents.Services.Logging;
using BlazorComponents.Services.Playwright;
using BlazorComponents.Services.YoutubeDLService;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorComponents.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorComponentsServices(this IServiceCollection services)
    {
        // Register component services
        services.AddSingleton<IAnimeService, AnimeService>();
        services.AddSingleton<IDirectorySelectionService, DirectorySelectionService>();
        services.AddSingleton<UserSettingsProvider>();
        services.AddApplicationLogging();
        services.AddYoutubeDL();
        services.AddPlaywright();
        services.AddAppInitializers();
        return services;
    }
}