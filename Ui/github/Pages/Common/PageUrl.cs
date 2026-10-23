namespace Ui.Github.Pages.Common;

internal static class PageUrl
{
    internal static int? TryGetResourceNumber(string url)
    {
        var lastSegment = url.Split('?', '#')[0].TrimEnd('/').Split('/').Last();

        return int.TryParse(lastSegment, out var number) ? number : null;
    }
}
