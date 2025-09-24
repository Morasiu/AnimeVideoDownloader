using BlazorComponents.Services.AppData;
using BlazorComponents.Services.Initializers;
using Microsoft.Extensions.Logging;
using YoutubeDLSharp;

namespace BlazorComponents.Services.YoutubeDLService;

public sealed class YoutubeDLInitializer : IInitializer
{
    private readonly YoutubeDL _youtubeDl;
    private readonly ILogger<YoutubeDLInitializer> _logger;
    public static string LibrariesPath => Path.Combine(AppDataPath.AnimeDownloaderPath, "libs");

    public YoutubeDLInitializer(YoutubeDL youtubeDl, ILogger<YoutubeDLInitializer> logger)
    {
        _youtubeDl = youtubeDl;
        _logger = logger;
    }

    public bool IsInitialized { get; set; }

    public async Task InitAsync()
    {
        if (IsInitialized) return;
        Directory.CreateDirectory(LibrariesPath);
        _logger.LogInformation("Downloading youtube-dl");
        await Utils.DownloadBinaries(skipExisting: true, LibrariesPath);
        _logger.LogInformation("Downloading youtube-dl finished");
        _logger.LogInformation("Updating youtube-dl");
        var result = await _youtubeDl.RunUpdate();
        _logger.LogInformation("Updating youtube-dl finished. Message {Message}", result);
        IsInitialized = true;
    }
}