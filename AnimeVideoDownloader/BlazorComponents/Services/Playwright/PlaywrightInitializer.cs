using BlazorComponents.Services.Initializers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace BlazorComponents.Services.Playwright;

public sealed class PlaywrightInitializer : IInitializer
{
    private readonly ILogger<PlaywrightInitializer> _logger;
    private readonly IServiceProvider _serviceProvider;


    public PlaywrightInitializer(ILogger<PlaywrightInitializer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public bool IsInitialized { get; set; }

    public Task InitAsync()
    {
        if (IsInitialized) return Task.CompletedTask;
        _logger.LogInformation("Installing Playwright");
        int exitCode = Program.Main(["install", "chromium"]);
        if (exitCode != 0)
        {
            _logger.LogError("Failed to install Playwright with exit code {ExitCode}", exitCode);
        }
        _logger.LogInformation("Installing Playwright finished. Code {Code}", exitCode);
        _logger.LogInformation("Creating Playwright Browser");
        var browserProvider = _serviceProvider.GetRequiredService<IBrowserProvider>();
        browserProvider.GetBrowser();
        _logger.LogInformation("Creating Playwright Browser finished");
        IsInitialized = true;
        return Task.CompletedTask;
    }
}