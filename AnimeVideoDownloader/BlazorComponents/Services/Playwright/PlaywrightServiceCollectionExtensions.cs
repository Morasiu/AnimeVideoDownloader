using Microsoft.Extensions.DependencyInjection;

namespace BlazorComponents.Services.Playwright;

public static class PlaywrightServiceCollectionExtensions
{
    public static IServiceCollection AddPlaywright(this IServiceCollection services)
    {
        services.AddSingleton<IBrowserProvider, BrowserProvider>();
        return services;
    }
}