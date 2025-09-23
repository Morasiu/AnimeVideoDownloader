namespace BlazorComponents.Services.Data.Models.EpisodeSources;

public enum Language
{
    Unknown,
    English,
    Polish,
    Japanese,
    Chinese
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