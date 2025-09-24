namespace BlazorComponents.Services.Data.Models.EpisodeSources;

public enum Language
{
    Unknown = 0,
    English = 1,
    Polish = 2,
    Japanese = 3,
    Chinese = 4,
}

public class LanguageParser
{
    public static Language Parse(string s)
    {
        return s.ToLowerInvariant().Trim() switch
        {
            "angielski" => Language.English,
            "polski" => Language.Polish,
            "japoński" => Language.Japanese,
            "chiński" => Language.Chinese,
            _ => Language.Unknown,
        };
    }
}