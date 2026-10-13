using Core.Config;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Repository;

internal sealed class NotFoundPage
{
    private readonly IPage _page;

    internal NotFoundPage(IPage page) => _page = page;

    internal async Task OpenAsync()
    {
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/gredjapl/ui-sandbox-nonexistent-xyz404");
    }

    internal async Task<bool> Has404MessageAsync() =>
        await _page.GetByAltText("404 “This is not the web page you are looking for”").CountAsync() > 0;
}
