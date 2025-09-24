using Microsoft.Extensions.Logging;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace BlazorComponents.Services.YoutubeDLService;

public sealed class DownloaderService
{
    private readonly ILogger<DownloaderService> _logger;
    private readonly YoutubeDL _youtubeDL;

    public const string Done = "Done";
    
    public DownloaderService(ILogger<DownloaderService> logger)
    {
        _logger = logger;
        var ytdl = new YoutubeDL
        {
            YoutubeDLPath = Path.Combine(YoutubeDLInitializer.LibrariesPath, Utils.YtDlpBinaryName),
            FFmpegPath = Path.Combine(YoutubeDLInitializer.LibrariesPath, Utils.FfmpegBinaryName),
        };
        _youtubeDL = ytdl;
    }

    public async Task<string> DownloadAsync(string url, string directoryPath, IProgress<AnimeDownloadDownloadProgress>? downloadProgress = null, CancellationToken ct = default)
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
            return string.Empty;
        }
        downloadProgress?.Report(new AnimeDownloadDownloadProgress()
        {
            Progress = 1,
            Status = Done,
            DownloadSpeed = null,
            Error = null,
            ETA = null,
            TotalBytes = null,
        });
        _logger.LogInformation("Download finished");
        return result.Data;
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