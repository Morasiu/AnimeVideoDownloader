using BlazorComponents.Services.Data;
using BlazorComponents.Services.Playwright;
using BlazorComponents.Services.YoutubeDLService;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorComponents.Services.Initializers;

public static class InitializersServiceCollectionExtensions
{
    public static IServiceCollection AddAppInitializers(this IServiceCollection services)
    {
        services.AddSingleton<AppInitializer>();
        services.AddSingleton<IInitializer, PlaywrightInitializer>();
        services.AddSingleton<IInitializer, YoutubeDLInitializer>();
        return services;
    }
}