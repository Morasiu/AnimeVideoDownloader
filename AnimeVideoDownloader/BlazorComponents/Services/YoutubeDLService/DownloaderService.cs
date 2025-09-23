using Microsoft.Extensions.Logging;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace BlazorComponents.Services.YoutubeDLService;

public sealed class DownloaderService
{
    private readonly ILogger<DownloaderService> _logger;
    private readonly YoutubeDL _youtubeDL;

    public DownloaderService(ILogger<DownloaderService> logger)
    {
        _logger = logger;
        var ytdl = new YoutubeDL();
        ytdl.YoutubeDLPath = Path.Combine(YoutubeDLInitializer.LibrariesPath, Utils.YtDlpBinaryName);
        ytdl.FFmpegPath = Path.Combine(YoutubeDLInitializer.LibrariesPath, Utils.FfmpegBinaryName);
        _youtubeDL = ytdl;
    }

    public async Task DownloadAsync(string url, string directoryPath, IProgress<AnimeDownloadDownloadProgress>? downloadProgress = null, CancellationToken ct = default)
    {
        var progress = new Progress<DownloadProgress>(p =>
        {
            downloadProgress?.Report(new AnimeDownloadDownloadProgress
            {
                TotalBytes = p.TotalDownloadSize,
                Progress = p.Progress,
                DownloadSpeed = p.DownloadSpeed,
                ETA = p.ETA,
                Status = p.State.ToString(),
                Error = null,
            });
        });
        _youtubeDL.OutputFolder = directoryPath;
        var options = OptionSet.Default;
        options.Progress = true;
        _logger.LogInformation("Starting downloading from {Url}", url);
        var result = await _youtubeDL.RunVideoDownload(url, progress: progress, ct: ct, overrideOptions: options);
        if (!result.Success)
        {
            downloadProgress?.Report(new AnimeDownloadDownloadProgress
            {
                DownloadSpeed = null,
                ETA = null,
                Progress = 0,
                Status = "Error",
                Error = string.Join(Environment.NewLine, result.ErrorOutput),
                TotalBytes = null,
            });
            _logger.LogError("Download failed with error {Error}", string.Join(Environment.NewLine, result.ErrorOutput));
        }
        _logger.LogInformation("Download finished");
    }
}

public class AnimeDownloadDownloadProgress
{
    public required string? TotalBytes { get; set; }
    public float Progress { get; set; }
    public required string? DownloadSpeed { get; set; }
    public required string? ETA { get; set; }
    
    public required string Status { get; set; }
    
    public required string? Error { get; set; }
}