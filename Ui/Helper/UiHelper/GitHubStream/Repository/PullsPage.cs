using Core.Config;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Repository;

internal sealed class PullsPage
{
    private readonly IPage _page;

    internal PullsPage(IPage page) => _page = page;

    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/pulls");

    internal async Task<bool> HasEmptyStateAsync() =>
        await _page.GetByRole(AriaRole.Heading, new() { Name = "No pull requests matched your search" }).CountAsync() > 0;

    internal async Task<int> GetOpenCountAsync() => await GetTabCountAsync("Open");

    internal async Task<int> GetClosedCountAsync() => await GetTabCountAsync("Closed");

    private async Task<int> GetTabCountAsync(string tabName)
    {
        var text = await _page.GetByRole(AriaRole.Link)
            .Filter(new() { HasTextString = tabName })
            .First.TextContentAsync();

        return TabCountParser.Parse(text);
    }
}
