using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;
using Ui.Helpers;

namespace Ui.Github.Pages.GitHubStream.Repository;

internal sealed class IssuesPage(IPage page)
{
    private readonly IPage _page = page;

    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/issues");

    internal async Task<bool> HasEmptyStateAsync() =>
        await _emptyStateHeading.CountAsync() > 0;

    internal async Task<bool> HasNewIssueButtonAsync() =>
        await _newIssueLink.CountAsync() > 0;

    internal async Task<bool> HasIssueWithTitleAsync(string title) =>
        await _issueLinks.Filter(new() { HasTextString = title }).CountAsync() > 0;

    // The list is indexed asynchronously after a write: a page loaded right after a create
    // can still miss the new issue — poll with a reload instead of a single instant count
    internal async Task<WaitResult<bool>> WaitForIssueWithTitleAsync(string title) =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                await _page.ReloadAsync();

                return await HasIssueWithTitleAsync(title);
            },
            found => found);

    internal Task<WaitResult<bool>> SearchAsync(string keyword) =>
        RetryHelper.UntilAsync(
            () => _page.Url.Contains(keyword, StringComparison.OrdinalIgnoreCase),
            async () =>
            {
                await _searchCombobox.FillAsync(keyword);
                await _searchButton.ClickAsync();
            });

    // the tab click can be swallowed before the list app hydrates — the link then falls back
    // to its href "#" and no navigation happens at all, so re-apply until the URL filters
    internal Task<WaitResult<bool>> ClickClosedTabAsync() =>
        RetryHelper.UntilAsync(
            () => _page.Url.Contains("closed"),
            () => _closedTabLink.ClickAsync());

    private readonly ILocator _emptyStateHeading = page.GetByRole(AriaRole.Heading, new() { Name = "No results matched your search" });
    private readonly ILocator _newIssueLink = page.GetByRole(AriaRole.Link, new() { Name = "New issue" });
    private readonly ILocator _issueLinks = page.GetByRole(AriaRole.Link);
    private readonly ILocator _closedTabLink = page.GetByRole(AriaRole.Link, new() { Name = "Closed", Exact = true });
    private readonly ILocator _searchCombobox = page.GetByRole(AriaRole.Combobox, new() { Name = "Search issues" });
    private readonly ILocator _searchButton = page.GetByRole(AriaRole.Button, new() { Name = "Search", Exact = true });
}
