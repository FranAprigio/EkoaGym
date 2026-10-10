using System.Globalization;

namespace EkoaGym.Mobile.Services.Localization;

public static class AppLanguages
{
    public const string English = "en";
    public const string PortugueseBrazil = "pt-BR";

    public static IReadOnlyList<string> All { get; } = [English, PortugueseBrazil];

    // First launch: any Portuguese locale (pt, pt-PT, pt-BR...) maps to pt-BR, everything else to English.
    public static string Resolve(string? deviceCultureName)
    {
        var name = deviceCultureName ?? string.Empty;
        var isPortuguese = name.Equals("pt", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("pt-", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("pt_", StringComparison.OrdinalIgnoreCase);
        return isPortuguese ? PortugueseBrazil : English;
    }

    // Returns the canonical code for a supported language, or null for anything else.
    public static string? Normalize(string? code) =>
        All.FirstOrDefault(language => language.Equals(code, StringComparison.OrdinalIgnoreCase));

    public static CultureInfo ToCulture(string code) =>
        CultureInfo.GetCultureInfo(code == PortugueseBrazil ? "pt-BR" : "en-US");
}
