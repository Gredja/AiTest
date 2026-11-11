namespace Ui.Helpers;

internal static class PageUrlHelper
{
    internal static int? TryGetResourceNumber(string url)
    {
        var cutIndex = url.IndexOfAny(['?', '#']);
        var path = (cutIndex >= 0 ? url[..cutIndex] : url).TrimEnd('/');
        var lastSlash = path.LastIndexOf('/');
        var lastSegment = lastSlash >= 0 ? path[(lastSlash + 1)..] : path;

        return int.TryParse(lastSegment, out var number) ? number : null;
    }
}
