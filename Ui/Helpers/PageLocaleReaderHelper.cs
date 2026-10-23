using Microsoft.Playwright;

namespace Ui.Helpers;

internal static class PageLocaleReaderHelper
{
    internal static Task<string?> ReadLangAsync(IPage page) =>
        page.GetAttributeAsync("html", "lang");

    internal static Task<string?> ReadDirAsync(IPage page) =>
        page.GetAttributeAsync("html", "dir");
}
