namespace BlazorComponents.Services.Data.Models.EpisodeSources;

public enum SourceKind
{
    Unkown,
    Mega,
    Cda,
    Gdrive,
    Mp4Upload,
    Sibnet,
    Yourupload,
}

public class SourceKindParser
{
    public static SourceKind Parse(string s)
    {
        return s.ToLowerInvariant().Trim() switch
        {
            "mega" => SourceKind.Mega,
            "cda" => SourceKind.Cda,
            "gdrive" => SourceKind.Gdrive,
            "mp4upload" => SourceKind.Mp4Upload,
            "sibnet" => SourceKind.Sibnet,
            "yourupload" => SourceKind.Yourupload,
            _ => SourceKind.Unkown,
        };
    }
}