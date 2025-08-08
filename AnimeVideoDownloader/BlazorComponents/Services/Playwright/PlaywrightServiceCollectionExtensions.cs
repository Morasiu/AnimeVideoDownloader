using BlazorComponents.Services.Playwright.Drivers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace BlazorComponents.Services.Playwright;

public static class PlaywrightServiceCollectionExtensions
{
    public static IServiceCollection AddPlaywright(this IServiceCollection services)
    {
        services.AddSingleton<IBrowser>(_ => ChromeDriverFactory.CreateNewAsync().GetAwaiter().GetResult());
        
        return services;
    }
}