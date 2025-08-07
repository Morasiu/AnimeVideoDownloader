using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BlazorComponents.Services.AppData;

public class UserSettingsProvider
{
    private readonly ILogger<UserSettingsProvider> _logger;
    private static string _userSettingsPath => Path.Combine(AppDataPath.AnimeDownloaderPath, "userSettings.json");

    public UserSettings Settings { get; private set; }

    public UserSettingsProvider(ILogger<UserSettingsProvider> logger)
    {
        _logger = logger;
        Settings = GetSettings();
    }

    private UserSettings GetSettings()
    {
        _logger.LogDebug("Reading settings from {UserSettingsPath}", _userSettingsPath);
        if (!File.Exists(_userSettingsPath))
        {
            _logger.LogDebug("User settings file does not exist");
            return new UserSettings();
        }
        var json = File.ReadAllText(_userSettingsPath);
        var settings = JsonSerializer.Deserialize<UserSettings>(json);
        if (settings == null)
        {
            throw new InvalidOperationException($"Could not deserialize settings from {_userSettingsPath}");
        }
        _logger.LogDebug("User settings loaded");
        return settings;
    }

    public void SaveSettings(UserSettings settings)
    {
        _logger.LogDebug("Saving settings to {UserSettingsPath}", _userSettingsPath);
        if (!Directory.Exists(AppDataPath.AnimeDownloaderPath))
        {
            Directory.CreateDirectory(AppDataPath.AnimeDownloaderPath);
        }
        var json = JsonSerializer.Serialize(settings);
        File.WriteAllText(_userSettingsPath, json);
        Settings = settings;
        _logger.LogDebug("User settings saved");
    }
}

public sealed class UserSettings
{
    public string DefaultQuality { get; set; } = "1080p";
    public string Theme { get; set; } = "system";
}