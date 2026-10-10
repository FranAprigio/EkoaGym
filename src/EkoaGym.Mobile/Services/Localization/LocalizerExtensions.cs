using System.Globalization;
using Microsoft.Extensions.Localization;

namespace EkoaGym.Mobile.Services.Localization;

public static class PluralRules
{
    // CLDR: Portuguese treats 0 and 1 as singular; English only 1.
    public static bool IsSingular(CultureInfo culture, int count)
    {
        var n = Math.Abs(count);
        return culture.TwoLetterISOLanguageName == "pt" ? n <= 1 : n == 1;
    }
}

public static class LocalizerExtensions
{
    // Looks up "{baseKey}.One" or "{baseKey}.Other" and formats it with the count as {0}.
    public static string Plural(this IStringLocalizer localizer, string baseKey, int count)
    {
        var form = PluralRules.IsSingular(CultureInfo.CurrentUICulture, count) ? "One" : "Other";
        return localizer[$"{baseKey}.{form}", count].Value;
    }
}
