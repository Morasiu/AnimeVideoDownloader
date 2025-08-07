using BlazorComponents.Services.AppData;
using BlazorComponents.Services.Initializers;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.YoutubeDLService;

public sealed class YoutubeDLInitializer : IInitializer
{
    private readonly ILogger<YoutubeDLInitializer> _logger;
    public static string LibrariesPath => Path.Combine(AppDataPath.AnimeDownloaderPath, "libs"); 

    public YoutubeDLInitializer(ILogger<YoutubeDLInitializer> logger)
    {
        _logger = logger;
    }
    
    public async Task InitAsync()
    {
        Directory.CreateDirectory(LibrariesPath);
        _logger.LogInformation("Downloading youtube-dl");
        await YoutubeDLSharp.Utils.DownloadBinaries(skipExisting: true, LibrariesPath);
        _logger.LogInformation("Downloading youtube-dl finished");
    }
}