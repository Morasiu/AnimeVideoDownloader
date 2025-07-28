using BlazorComponents.Components.Anime.Services;
using BlazorComponents.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Extensions
{
    public static class ServiceCollectionExtensions
    {
        // Track whether the debug logger has been registered
        private static bool _debugLoggerRegistered = false;

        // Store a singleton instance of the provider
        private static DebugLoggerProvider? _debugLoggerProvider = null;

        public static IServiceCollection AddBlazorComponentsServices(this IServiceCollection services)
        {
            // Register component services
            services.AddSingleton<IAnimeService, AnimeService>();
            services.AddLogging(builder => {
                builder.SetMinimumLevel(LogLevel.Information);

                // Register the DebugLoggerProvider with the logging system
                if (!_debugLoggerRegistered)
                {
                    builder.Services.AddSingleton<DebugLoggerProvider>(sp => {
                        if (_debugLoggerProvider == null)
                        {
                            _debugLoggerProvider = new DebugLoggerProvider((level, category, message) => {
                                // This will be set later by the Debug component
                                DebugLoggerProvider.LogMessageAction?.Invoke(level, category, message);
                            });
                        }
                        return _debugLoggerProvider;
                    });

                    _debugLoggerRegistered = true;
                }
            });
            
            return services;
        }
    }
}
