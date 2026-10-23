using Microsoft.Playwright;

namespace Ui.Moodledemo.Tests.Support;

internal static class PageLocaleReader
{
    internal static Task<string?> ReadLangAsync(IPage page) =>
        page.GetAttributeAsync("html", "lang");

    internal static Task<string?> ReadDirAsync(IPage page) =>
        page.GetAttributeAsync("html", "dir");
}
