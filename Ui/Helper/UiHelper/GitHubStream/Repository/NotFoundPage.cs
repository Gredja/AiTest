using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Repository;

internal sealed class NotFoundPage
{
    // matches the fixture's action timeout (GitHubUiTestBase.UiTimeoutMs)
    private static readonly TimeSpan MessageTimeout = TimeSpan.FromSeconds(15);

    private readonly IPage _page;

    internal NotFoundPage(IPage page) => _page = page;

    internal async Task OpenAsync()
    {
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/gredjapl/ui-sandbox-nonexistent-xyz404");
    }

    // this page is outside the load-verified list in Rules/ui-testing.md → Waits,
    // so the presence check polls instead of reading the count instantly
    internal async Task<WaitResult<bool>> WaitFor404MessageAsync() =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                var count = await _page.GetByAltText("404 “This is not the web page you are looking for”").CountAsync();
                return count > 0;
            },
            hasMessage => hasMessage,
            MessageTimeout);
}
