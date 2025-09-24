namespace BlazorComponents.Services.Data.Models.EpisodeSources;

public enum SourceKind
{
    Unkown = 0,
    Mega = 1,
    Cda = 2,
    Gdrive = 3,
    Mp4Upload = 4,
    Sibnet = 5,
    Yourupload = 6,
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