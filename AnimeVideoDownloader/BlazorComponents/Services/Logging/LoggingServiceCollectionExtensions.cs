using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.Logging;

public static class LoggingServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLogging(this IServiceCollection services)
    {
        services.AddSingleton<MemoryLogService>();
        services.AddLogging(loggingBuilder =>
        {
            var serviceProvider = loggingBuilder.Services.BuildServiceProvider();
            var memoryLogService = serviceProvider.GetRequiredService<MemoryLogService>();
            var memoryLoggerProvider = new MemoryLoggerProvider(memoryLogService);
            loggingBuilder.AddProvider(memoryLoggerProvider);
        });
        return services;
    }
}