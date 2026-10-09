using Microsoft.Playwright;
using Ui.Helper.UiHelper.CommonPages;

namespace Ui.Helper.BrowserHelpers;

// Page-object wrapper (XRM_Autotest pattern): every page-object receives the wrapper
// in its constructor; pages are exposed here as properties as soon as they exist
// (added by ui-test-gen).
internal sealed class BrowserPw
{
    internal BrowserPw(IPage page)
    {
        Page = page;
        LoginPage = new LoginPage(page);
        HeaderPage = new HeaderPage(page);
    }

    internal IPage Page { get; }
    internal LoginPage LoginPage { get; }
    internal HeaderPage HeaderPage { get; }
}
