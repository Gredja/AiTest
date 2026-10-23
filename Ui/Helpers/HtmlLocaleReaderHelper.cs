using System.Text.RegularExpressions;
using FluentAssertions;

namespace Ui.Helpers;

internal static class HtmlLocaleReaderHelper
{
    // opening tag only: "<html dir=... lang=...>" — carries the locale attributes
    private static readonly Regex _htmlTagPattern = new(
        @"<html[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // attribute preceded by whitespace so xml:lang="..." never matches the bare lang check
    private static readonly Regex _langPattern = new(
        @"\slang=""([^""]*)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // dir attribute preceded by whitespace so data-dir="..." never matches the bare dir check
    private static readonly Regex _dirPattern = new(
        @"\sdir=""([^""]*)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // full <title> element — captures the inner text
    private static readonly Regex _titlePattern = new(
        @"<title>([^<]*)</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    internal static (string Lang, string Dir) ReadLocale(string html)
    {
        var htmlTag = _htmlTagPattern.Match(html);
        htmlTag.Success.Should().BeTrue("response must contain an <html> tag");

        var lang = _langPattern.Match(htmlTag.Value);
        lang.Success.Should().BeTrue("<html> tag must carry a lang attribute");

        var dir = _dirPattern.Match(htmlTag.Value);
        dir.Success.Should().BeTrue("<html> tag must carry a dir attribute");

        return (lang.Groups[1].Value, dir.Groups[1].Value);
    }

    internal static string ReadTitle(string html)
    {
        var title = _titlePattern.Match(html);
        title.Success.Should().BeTrue("response must contain a <title> element");

        return title.Groups[1].Value;
    }
}
