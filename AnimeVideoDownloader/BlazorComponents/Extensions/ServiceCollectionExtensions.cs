using BlazorComponents.Components.Anime.Services;
using BlazorComponents.Drivers;
using BlazorComponents.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace BlazorComponents.Extensions
{
    public static class ServiceCollectionExtensions
    {


        public static IServiceCollection AddBlazorComponentsServices(this IServiceCollection services)
        {
            // Register component services
            services.AddSingleton<IAnimeService, AnimeService>();
            services.AddSingleton<IBrowser>(_ => ChromeDriverFactory.CreateNewAsync().GetAwaiter().GetResult());
            services.AddSingleton<MemoryLogService>();
            services.AddLogging(loggingBuilder =>
            {
                var serviceProvider = loggingBuilder.Services.BuildServiceProvider();
                var memoryLogService = serviceProvider.GetRequiredService<MemoryLogService>();
                var memoryLoggerProvider = new MemoryLoggerProvider(memoryLogService);
                loggingBuilder.AddProvider(memoryLoggerProvider);
            });
            services.AddTransient<PlaywrightInitBackgroundService>();
            return services;
        }
    }
}