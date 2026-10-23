using System.Text.RegularExpressions;
using FluentAssertions;

namespace Ui.Moodledemo.Tests.Support;

internal static class TitleScriptAssert
{
    // .NET regex has no \p{IsLatin}-style script properties — only Unicode blocks,
    // so scripts are expressed as unions of the blocks that hold their letters.
    private static readonly IReadOnlyDictionary<string, string> _scriptPatterns =
        new Dictionary<string, string>
        {
            // IPA Extensions carries ə (U+0259), used as a Latin letter in Azerbaijani etc.
            ["Latin"] = @"\p{IsBasicLatin}\p{IsLatin-1Supplement}\p{IsLatinExtended-A}\p{IsLatinExtended-B}\p{IsIPAExtensions}\p{IsLatinExtendedAdditional}",
            ["Cyrillic"] = @"\p{IsCyrillic}",
            ["Greek"] = @"\p{IsGreek}",
            ["Arabic"] = @"\p{IsArabic}\p{IsArabicPresentationForms-A}",
            ["Hebrew"] = @"\p{IsHebrew}",
            ["Thaana"] = @"\p{IsThaana}",
            ["Tibetan"] = @"\p{IsTibetan}",
            ["Devanagari"] = @"\p{IsDevanagari}",
            ["Bengali"] = @"\p{IsBengali}",
            ["Gujarati"] = @"\p{IsGujarati}",
            ["Gurmukhi"] = @"\p{IsGurmukhi}",
            ["Kannada"] = @"\p{IsKannada}",
            ["Malayalam"] = @"\p{IsMalayalam}",
            ["Sinhala"] = @"\p{IsSinhala}",
            ["Tamil"] = @"\p{IsTamil}",
            ["Telugu"] = @"\p{IsTelugu}",
            ["Thai"] = @"\p{IsThai}",
            ["Lao"] = @"\p{IsLao}",
            ["Khmer"] = @"\p{IsKhmer}",
            ["Armenian"] = @"\p{IsArmenian}",
            ["Georgian"] = @"\p{IsGeorgian}",
            ["Ethiopic"] = @"\p{IsEthiopic}",
            ["Han"] = @"\p{IsCJKUnifiedIdeographs}\p{IsCJKUnifiedIdeographsExtensionA}",
            ["Hangul"] = @"\p{IsHangulSyllables}",
            ["Hiragana"] = @"\p{IsHiragana}",
            ["Katakana"] = @"\p{IsKatakana}",
            ["Mongolian"] = @"\p{IsMongolian}",
        };

    // The site name suffix ("| Mount Orange") is branding, not translation — the script
    // rule applies to the translatable part only.
    internal static void ShouldMatch(string title, string code)
    {
        var translatablePart = title.Split('|')[0];
        var expectedScripts = MoodledemoFixtures.TitleScriptsFor(code);
        var pattern = BuildPattern(expectedScripts, code);
        var regex = new Regex(pattern, RegexOptions.Compiled);

        var offenders = translatablePart
            .Where(char.IsLetter)
            .Distinct()
            .Where(letter => !regex.IsMatch(letter.ToString()))
            .ToList();

        offenders.Should().BeEmpty(
            "every letter of the '{0}' title part '{1}' must belong to scripts [{2}]; offending letters: [{3}]",
            code,
            translatablePart.Trim(),
            string.Join(", ", expectedScripts),
            string.Join(", ", offenders));
    }

    private static string BuildPattern(IReadOnlyList<string> scripts, string code)
    {
        var classes = scripts.Select(script => _scriptPatterns.TryGetValue(script, out var pattern)
            ? pattern
            : throw new InvalidOperationException(
                $"titleScripts: unknown script '{script}' for code '{code}'"));

        return $"[{string.Join(string.Empty, classes)}]";
    }
}
