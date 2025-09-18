using BlazorComponents.Services.AnimeService;
using BlazorComponents.Services.AppData;
using BlazorComponents.Services.Data;
using BlazorComponents.Services.DirectorySelection;
using BlazorComponents.Services.Initializers;
using BlazorComponents.Services.Logging;
using BlazorComponents.Services.Playwright;
using BlazorComponents.Services.Toasts;
using BlazorComponents.Services.YoutubeDLService;
using Microsoft.EntityFrameworkCore;
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
        services.AddSingleton<ToastService>();
        services.AddApplicationLogging();
        services.AddYoutubeDL();
        services.AddPlaywright();
        services.AddAppInitializers();
        services.AddDbContext<ApplicationDbContext>((_, o) => o
            .UseSqlite($"Data Source={AppDataPath.AnimeDownloaderPath}/anime.db")
            .EnableSensitiveDataLogging()
            .UseLazyLoadingProxies()
        );
        services.AddBlazorContextMenu();
        return services;
    }
}