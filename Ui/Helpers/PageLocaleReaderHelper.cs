using Microsoft.Playwright;

namespace Ui.Helpers;

internal static class PageLocaleReaderHelper
{
    private const string HtmlElement = "html";
    private const string LangAttribute = "lang";
    private const string DirAttribute = "dir";

    internal static Task<string?> ReadLangAsync(IPage page) =>
        page.GetAttributeAsync(HtmlElement, LangAttribute);

    internal static Task<string?> ReadDirAsync(IPage page) =>
        page.GetAttributeAsync(HtmlElement, DirAttribute);
}
