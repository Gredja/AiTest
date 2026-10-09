using Microsoft.Playwright;

namespace Ui.Helper.BrowserHelpers;

// Page-object wrapper (XRM_Autotest pattern): every page-object receives the wrapper
// in its constructor; pages are exposed here as properties as soon as they exist
// (added by ui-test-gen).
internal sealed class BrowserPw
{
    internal BrowserPw(IPage page) => Page = page;

    internal IPage Page { get; }
}
