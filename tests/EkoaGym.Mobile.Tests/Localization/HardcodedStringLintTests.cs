using System.Text;
using System.Text.RegularExpressions;

namespace EkoaGym.Mobile.Tests.Localization;

// Heuristic scan for user-facing literals that bypass IStringLocalizer. Files with legacy offenders are
// tracked in hardcoded-strings.baseline.txt, which can only shrink: new literals fail, and fixing one
// without lowering the baseline fails too, so the list ratchets down to empty.
public partial class HardcodedStringLintTests
{
    private static readonly string[] AllowedLiterals = ["EkoaGym", "Language / Idioma"];

    // Localization plumbing builds resource keys from literals; nothing in these files is shown to users.
    private static readonly string[] ExemptPaths = ["Services/Localization/", "Components/Shared/LocalizedDataAnnotationsValidator.cs"];

    private static string BaselinePath =>
        Path.Combine(RepoPaths.Root, "tests", "EkoaGym.Mobile.Tests", "Localization", "hardcoded-strings.baseline.txt");

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline)]
    private static partial Regex RazorComment();

    [GeneratedRegex(@"<(script|style)\b.*?</\1>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex ScriptOrStyle();

    [GeneratedRegex(@">([^<>]+)<")]
    private static partial Regex TextNode();

    [GeneratedRegex(@"(?<![\w-])(?:placeholder|title|aria-label|alt|Title|Description|Message|Label|Text|Placeholder|Caption)=""([^""@]*)""")]
    private static partial Regex LiteralAttribute();

    [GeneratedRegex(@"@\((?:[^()]|\((?:[^()]|\([^()]*\))*\))*\)")]
    private static partial Regex ExplicitExpression();

    [GeneratedRegex(@"@[A-Za-z_][\w.]*(?:\((?:[^()]|\([^()]*\))*\))?(?:\[[^\]]*\])?")]
    private static partial Regex ImplicitExpression();

    [GeneratedRegex(@"^\s*(@|\}|\{|else\b|case\b|default\b)")]
    private static partial Regex CodeLine();

    [GeneratedRegex(@"\p{L}{2,}")]
    private static partial Regex Word();

    // Ternary branches and switch arms that yield display text (capitalised, unlike CSS class names).
    [GeneratedRegex(@"(?:\?|:|=>)\s*\$?""(\s*\(?\p{Lu}\p{L}+[^""]*)""")]
    private static partial Regex DisplayBranch();

    [GeneratedRegex(
        @"(?:\b(?:errorMessage|\w+Message|Title|Description|Label)\s*=\s*\$?""([^""]*)""|new\s+ApiException\(\s*\$?""([^""]*)""|""confirm"",\s*\$?""([^""]*)""|AuthenticationRequest\(\s*\$?""([^""]*)"")")]
    private static partial Regex CodeLiteral();

    [Fact]
    public void NoNewHardcodedUserFacingStrings()
    {
        var findings = SourceFiles.Enumerate()
            .Where(file => !ExemptPaths.Any(exempt => SourceFiles.Relative(file).StartsWith(exempt, StringComparison.Ordinal)))
            .Select(file => (Path: SourceFiles.Relative(file), Hits: Scan(file).ToList()))
            .Where(result => result.Hits.Count > 0)
            .ToDictionary(result => result.Path, result => result.Hits);

        if (Environment.GetEnvironmentVariable("UPDATE_LOCALIZATION_BASELINE") == "1")
        {
            File.WriteAllLines(BaselinePath, findings.OrderBy(f => f.Key, StringComparer.Ordinal).Select(f => $"{f.Key}|{f.Value.Count}"));
            return;
        }

        var baseline = File.Exists(BaselinePath)
            ? File.ReadAllLines(BaselinePath)
                .Where(line => line.Contains('|'))
                .Select(line => line.Split('|'))
                .ToDictionary(parts => parts[0], parts => int.Parse(parts[1]))
            : [];

        var problems = new StringBuilder();
        foreach (var (path, hits) in findings)
        {
            var allowed = baseline.GetValueOrDefault(path);
            if (hits.Count > allowed)
            {
                problems.AppendLine($"{path}: {hits.Count} hardcoded string(s), baseline allows {allowed}. Use L[\"Key\"] instead:");
                foreach (var hit in hits)
                {
                    problems.AppendLine($"    {hit}");
                }
            }
        }

        foreach (var (path, allowed) in baseline)
        {
            var actual = findings.TryGetValue(path, out var hits) ? hits.Count : 0;
            if (actual < allowed)
            {
                problems.AppendLine($"{path}: baseline says {allowed} but only {actual} remain; lower the baseline (UPDATE_LOCALIZATION_BASELINE=1).");
            }
        }

        Assert.True(problems.Length == 0, problems.ToString());
    }

    private static IEnumerable<string> Scan(string file)
    {
        var text = File.ReadAllText(file);

        if (file.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
        {
            var codeIndex = text.LastIndexOf("@code", StringComparison.Ordinal);
            var markup = codeIndex >= 0 ? text[..codeIndex] : text;
            var code = codeIndex >= 0 ? text[codeIndex..] : string.Empty;

            foreach (var hit in ScanMarkup(markup))
            {
                yield return hit;
            }

            foreach (var hit in ScanCode(code))
            {
                yield return hit;
            }
        }
        else
        {
            foreach (var hit in ScanCode(text))
            {
                yield return hit;
            }
        }
    }

    private static IEnumerable<string> ScanMarkup(string markup)
    {
        markup = ScriptOrStyle().Replace(RazorComment().Replace(markup, string.Empty), string.Empty);

        foreach (Match match in LiteralAttribute().Matches(markup))
        {
            if (IsUserText(match.Groups[1].Value))
            {
                yield return match.Value.Trim();
            }
        }

        foreach (Match match in ExplicitExpression().Matches(markup))
        {
            foreach (Match branch in DisplayBranch().Matches(match.Value))
            {
                if (IsUserText(branch.Groups[1].Value))
                {
                    yield return branch.Value.Trim();
                }
            }
        }

        foreach (Match match in TextNode().Matches(markup))
        {
            foreach (var rawLine in match.Groups[1].Value.Split('\n'))
            {
                if (CodeLine().IsMatch(rawLine))
                {
                    continue;
                }

                var line = ImplicitExpression().Replace(ExplicitExpression().Replace(rawLine, " "), " ").Trim();
                if (IsUserText(line))
                {
                    yield return line;
                }
            }
        }
    }

    private static IEnumerable<string> ScanCode(string code)
    {
        foreach (Match branch in DisplayBranch().Matches(code))
        {
            if (IsUserText(branch.Groups[1].Value))
            {
                yield return branch.Value.Trim();
            }
        }

        foreach (Match match in CodeLiteral().Matches(code))
        {
            var literal = match.Groups.Cast<Group>().Skip(1).First(g => g.Success).Value;
            if (IsUserText(literal))
            {
                yield return match.Value.Trim();
            }
        }
    }

    private static bool IsUserText(string value) =>
        Word().IsMatch(value) && !AllowedLiterals.Contains(value.Trim(), StringComparer.Ordinal);
}
