using Core.Config;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Repository;

internal sealed class CodePage
{
    private readonly IPage _page;

    internal CodePage(IPage page) => _page = page;

    internal async Task OpenAsync()
    {
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/tree/main");
    }

    internal async Task<bool> HasReadmeAsync() =>
        await _page.GetByRole(AriaRole.Link, new() { Name = "README.md, (File)" }).CountAsync() > 0;
}
