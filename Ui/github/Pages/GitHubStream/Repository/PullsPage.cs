using Core.Config;
using Core.Helpers;
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

    internal async Task<bool> ContainsPullWithTitleAsync(string title) =>
        await _tabLinks.Filter(new() { HasTextString = title }).CountAsync() > 0;

    // the list is indexed asynchronously after a write: a page loaded right after a create
    // can still miss the new pull request — poll with a reload instead of a single count
    internal async Task<WaitResult<bool>> WaitForPullWithTitleAsync(string title) =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                await _page.ReloadAsync();

                return await ContainsPullWithTitleAsync(title);
            },
            found => found);

    private async Task<int> GetTabCountAsync(string tabName)
    {
        var text = await _tabLinks.Filter(new() { HasTextString = tabName }).First.TextContentAsync();

        return TabCountParser.Parse(text);
    }

    private readonly ILocator _emptyStateHeading = page.GetByRole(AriaRole.Heading, new() { Name = "No pull requests matched your search" });
    private readonly ILocator _tabLinks = page.GetByRole(AriaRole.Link);
}
