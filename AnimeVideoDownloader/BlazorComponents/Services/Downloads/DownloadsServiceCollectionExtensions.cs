using Microsoft.Extensions.DependencyInjection;

namespace BlazorComponents.Services.Downloads;

public static class DownloadsServiceCollectionExtensions
{
    public static IServiceCollection AddDownloads(this IServiceCollection services)
    {
        services.AddSingleton<DownloadQueueService>();
        return services;
    }
}