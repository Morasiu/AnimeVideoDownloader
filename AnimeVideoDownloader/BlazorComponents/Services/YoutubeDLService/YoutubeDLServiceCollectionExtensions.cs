using Microsoft.Extensions.DependencyInjection;

namespace BlazorComponents.Services.YoutubeDLService;

public static class YoutubeDLServiceCollectionExtensions
{
    public static IServiceCollection AddYoutubeDL(this IServiceCollection services)
    {
        services.AddSingleton<DownloaderService>();
        return services;
    }
}