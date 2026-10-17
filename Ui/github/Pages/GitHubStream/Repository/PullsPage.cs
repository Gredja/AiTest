using Core.Config;
using Microsoft.Playwright;
using Ui.Github.Tests.Support;

namespace Ui.Github.Pages.GitHubStream.Repository;

internal sealed class PullsPage(IPage page)
{
    private readonly IPage _page = page;

    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/pulls");

    internal async Task<bool> HasEmptyStateAsync() =>
        await _emptyStateHeading.CountAsync() > 0;

    internal async Task<int> GetOpenCountAsync() => await GetTabCountAsync("Open");

    internal async Task<int> GetClosedCountAsync() => await GetTabCountAsync("Closed");

    private async Task<int> GetTabCountAsync(string tabName)
    {
        var text = await _tabLinks.Filter(new() { HasTextString = tabName }).First.TextContentAsync();

        return TabCountParser.Parse(text);
    }

    private readonly ILocator _emptyStateHeading = page.GetByRole(AriaRole.Heading, new() { Name = "No pull requests matched your search" });
    private readonly ILocator _tabLinks = page.GetByRole(AriaRole.Link);
}
