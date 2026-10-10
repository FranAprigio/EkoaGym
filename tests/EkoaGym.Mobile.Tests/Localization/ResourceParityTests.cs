using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EkoaGym.Mobile.Tests.Localization;

internal static class ResxReader
{
    public static Dictionary<string, string> Load(string path) =>
        XDocument.Load(path).Root!
            .Elements("data")
            .ToDictionary(
                data => (string)data.Attribute("name")!,
                data => (string?)data.Element("value") ?? string.Empty);

    public static string[] Placeholders(string value) =>
        Regex.Matches(value, @"\{\d+\}").Select(m => m.Value).Distinct().OrderBy(p => p).ToArray();
}

public class ResourceParityTests
{
    private static readonly Dictionary<string, string> English = ResxReader.Load(RepoPaths.EnglishResx);
    private static readonly Dictionary<string, string> Portuguese = ResxReader.Load(RepoPaths.PortugueseResx);

    [Fact]
    public void BothFiles_HaveTheSameKeys()
    {
        var missingInPortuguese = English.Keys.Except(Portuguese.Keys).OrderBy(k => k).ToList();
        var missingInEnglish = Portuguese.Keys.Except(English.Keys).OrderBy(k => k).ToList();

        Assert.True(
            missingInPortuguese.Count == 0 && missingInEnglish.Count == 0,
            $"Missing in pt-BR: [{string.Join(", ", missingInPortuguese)}]; missing in en: [{string.Join(", ", missingInEnglish)}]");
    }

    [Fact]
    public void NoValueIsBlank()
    {
        var blank = English.Concat(Portuguese)
            .Where(pair => string.IsNullOrWhiteSpace(pair.Value))
            .Select(pair => pair.Key)
            .Distinct()
            .ToList();

        Assert.True(blank.Count == 0, $"Blank values (a blank pt-BR value would not fall back to English): {string.Join(", ", blank)}");
    }

    [Fact]
    public void PlaceholdersMatchBetweenLanguages()
    {
        var mismatched = English
            .Where(pair => Portuguese.TryGetValue(pair.Key, out var pt)
                && !ResxReader.Placeholders(pair.Value).SequenceEqual(ResxReader.Placeholders(pt)))
            .Select(pair => pair.Key)
            .ToList();

        Assert.True(mismatched.Count == 0, $"Placeholder mismatch: {string.Join(", ", mismatched)}");
    }

    [Fact]
    public void EveryPluralKeyHasBothForms()
    {
        var incomplete = English.Keys
            .Where(key => key.StartsWith("Plural.", StringComparison.Ordinal))
            .Select(key => key[..key.LastIndexOf('.')])
            .Distinct()
            .Where(baseKey => !English.ContainsKey($"{baseKey}.One") || !English.ContainsKey($"{baseKey}.Other"))
            .ToList();

        Assert.True(incomplete.Count == 0, $"Plural keys missing .One/.Other: {string.Join(", ", incomplete)}");
    }

    [Theory]
    [InlineData("Required")]
    [InlineData("EmailAddress")]
    [InlineData("StringLength")]
    [InlineData("MinLength")]
    [InlineData("MaxLength")]
    [InlineData("Range")]
    [InlineData("Compare")]
    public void ValidationKeyExistsForCommonAttributes(string attributeName) =>
        Assert.Contains($"Validation.{attributeName}", English.Keys);

    [Fact]
    public void EverySupportedLanguageHasADisplayName()
    {
        foreach (var language in EkoaGym.Mobile.Services.Localization.AppLanguages.All)
        {
            Assert.Contains($"Language.{language}", English.Keys);
        }
    }
}
