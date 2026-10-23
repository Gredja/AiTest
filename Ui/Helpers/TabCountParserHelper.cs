using System.Globalization;
using System.Text.RegularExpressions;

namespace Ui.Helpers;

internal static class TabCountParserHelper
{
    // tab counter text ends with its count: "Open (0)" (anonymous) / "Open 0" (authenticated)
    private static readonly Regex _trailingCountPattern = new(@"(\d+)\)?\s*$", RegexOptions.Compiled);

    internal static int? Parse(string? tabText)
    {
        var match = _trailingCountPattern.Match(tabText ?? string.Empty);

        return match.Success
            ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : null;
    }
}
