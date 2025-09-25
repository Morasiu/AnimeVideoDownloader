using Microsoft.Extensions.DependencyInjection;
using YoutubeDLSharp;

namespace BlazorComponents.Services.YoutubeDLService;

public static class YoutubeDLServiceCollectionExtensions
{
    public static IServiceCollection AddYoutubeDL(this IServiceCollection services)
    {
        services.AddSingleton<DownloaderService>();
        services.AddSingleton<YoutubeDL>(_ => new YoutubeDL
        {
            YoutubeDLPath = Path.Combine(YoutubeDLInitializer.LibrariesPath, Utils.YtDlpBinaryName),
            FFmpegPath = Path.Combine(YoutubeDLInitializer.LibrariesPath, Utils.FfmpegBinaryName),
        });
        return services;
    }
}