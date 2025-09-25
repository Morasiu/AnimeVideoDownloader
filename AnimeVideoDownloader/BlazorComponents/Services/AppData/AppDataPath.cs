namespace BlazorComponents.Services.AppData;

public sealed class AppDataPath
{
    private const string APPLICATION_DIRECTORY_NAME = "AnimeDownloader";
    public static string AnimeDownloaderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPLICATION_DIRECTORY_NAME);
}