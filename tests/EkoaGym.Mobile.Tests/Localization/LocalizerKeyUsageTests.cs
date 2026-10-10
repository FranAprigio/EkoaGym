using System.Text.RegularExpressions;

namespace EkoaGym.Mobile.Tests.Localization;

public partial class LocalizerKeyUsageTests
{
    [GeneratedRegex(@"\bL\[\s*""([^""]+)""")]
    private static partial Regex IndexerKey();

    [GeneratedRegex(@"\.Plural\(\s*""([^""]+)""")]
    private static partial Regex PluralKey();

    [Fact]
    public void EveryLiteralKeyUsedInSourceExistsInEnglishResx()
    {
        var english = ResxReader.Load(RepoPaths.EnglishResx);
        var missing = new List<string>();

        foreach (var file in SourceFiles.Enumerate())
        {
            var text = File.ReadAllText(file);
            foreach (Match match in IndexerKey().Matches(text))
            {
                if (!english.ContainsKey(match.Groups[1].Value))
                {
                    missing.Add($"{SourceFiles.Relative(file)}: {match.Groups[1].Value}");
                }
            }

            foreach (Match match in PluralKey().Matches(text))
            {
                var key = match.Groups[1].Value;
                if (!english.ContainsKey($"{key}.One") || !english.ContainsKey($"{key}.Other"))
                {
                    missing.Add($"{SourceFiles.Relative(file)}: {key}.One/.Other");
                }
            }
        }

        Assert.True(missing.Count == 0, "Keys used in code but absent from SharedResources.resx:\n" + string.Join("\n", missing));
    }
}

internal static class SourceFiles
{
    // Everything that can render user-facing text; generated and build output is excluded.
    public static IEnumerable<string> Enumerate()
    {
        var project = RepoPaths.MobileProject;
        var roots = new[] { Path.Combine(project, "Components"), Path.Combine(project, "Services") };

        return roots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
            .Where(file => file.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)
                || (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && !file.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .OrderBy(file => file, StringComparer.Ordinal);
    }

    public static string Relative(string file) =>
        Path.GetRelativePath(RepoPaths.MobileProject, file).Replace('\\', '/');
}
