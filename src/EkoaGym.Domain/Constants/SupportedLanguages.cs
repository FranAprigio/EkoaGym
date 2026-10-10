namespace EkoaGym.Domain.Constants;

public static class SupportedLanguages
{
    public const string English = "en";
    public const string PortugueseBrazil = "pt-BR";
    public const string Default = English;

    public static IReadOnlyList<string> All { get; } = [English, PortugueseBrazil];

    // Language of the current request (set from Accept-Language by request localization), English when unknown.
    public static string Current => Normalize(System.Globalization.CultureInfo.CurrentUICulture.Name) ?? Default;

    // Canonical code for a supported language ("pt", "pt-br" -> "pt-BR"), or null when unsupported.
    public static string? Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var trimmed = code.Trim();
        if (trimmed.Equals(English, StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("en-", StringComparison.OrdinalIgnoreCase))
        {
            return English;
        }

        if (trimmed.Equals("pt", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("pt-", StringComparison.OrdinalIgnoreCase))
        {
            return PortugueseBrazil;
        }

        return null;
    }
}
