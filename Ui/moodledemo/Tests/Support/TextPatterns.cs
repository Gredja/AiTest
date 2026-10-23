using System.Text.RegularExpressions;

namespace Ui.Moodledemo.Tests.Support;

internal static class TextPatterns
{
    // collapses runs of whitespace (newlines/tabs/multiple spaces) to one space and trims —
    // text comparisons must not depend on layout wrapping
    private static readonly Regex _collapseWhitespace = new(@"\s+", RegexOptions.Compiled);

    internal static string CollapseWhitespace(string? text) =>
        _collapseWhitespace.Replace(text ?? string.Empty, " ").Trim();
}
