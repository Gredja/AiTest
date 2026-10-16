using System.Text.RegularExpressions;

namespace Ui.Tests.Support;

internal static class TabCountParser
{
    // tab counter text ends with its count: "Open (0)" (anonymous) / "Open 0" (authenticated)
    private static readonly Regex TrailingCountPattern = new(@"(\d+)\)?\s*$", RegexOptions.Compiled);

    internal static int Parse(string? tabText)
    {
        var match = TrailingCountPattern.Match(tabText ?? string.Empty);

        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
    }
}
