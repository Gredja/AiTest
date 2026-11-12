using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;
using Ui.Helpers;

namespace Ui.Github.Pages.GitHubStream.Repository;

internal sealed class IssuesPage(IPage page)
{
    internal async Task OpenAsync() => await page.GotoAsync($"{TestConfig.UiBaseUrl}{GitHubUiEndpoints.IssuesPath}");

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
                await page.ReloadAsync();

                return await HasIssueWithTitleAsync(title);
            },
            found => found);

    // after an API-driven close-all the list lags the writes — poll with a reload
    // instead of sampling the empty heading once
    internal async Task<WaitResult<bool>> WaitForEmptyStateAsync() =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                await page.ReloadAsync();

                return await HasEmptyStateAsync();
            },
            isEmpty => isEmpty);

    internal Task<WaitResult<bool>> SearchAsync(string keyword) =>
        RetryHelper.UntilAsync(
            () => page.Url.Contains(keyword, StringComparison.OrdinalIgnoreCase),
            async () =>
            {
                await _searchCombobox.FillAsync(keyword);
                await _searchButton.ClickAsync();
            });

    // the tab click can be swallowed before the list app hydrates — the link then falls back
    // to its href "#" and no navigation happens at all, so re-apply until the URL filters
    internal Task<WaitResult<bool>> ClickClosedTabAsync() =>
        RetryHelper.UntilAsync(
            () => page.Url.Contains(GitHubEndpoints.StateClosed),
            () => _closedTabLink.ClickAsync());

    internal async Task<bool> HasIssuesHeadingAsync() =>
        await _issuesHeading.CountAsync() > 0;

    internal async Task<bool> HasOpenTabAsync() =>
        await _openTabLink.CountAsync() > 0;

    private async Task<bool> HasEmptyStateAsync() =>
        await _emptyStateHeading.CountAsync() > 0;

    private readonly ILocator _emptyStateHeading = page.GetByRole(AriaRole.Heading, new() { Name = "No results matched your search" });
    private readonly ILocator _newIssueLink = page.GetByRole(AriaRole.Link, new() { Name = "New issue" });
    private readonly ILocator _issueLinks = page.GetByRole(AriaRole.Link);
    private readonly ILocator _closedTabLink = page.GetByRole(AriaRole.Link, new() { Name = GitHubStateLabels.Closed, Exact = true });
    private readonly ILocator _searchCombobox = page.GetByRole(AriaRole.Combobox, new() { Name = "Search issues" });
    private readonly ILocator _searchButton = page.GetByRole(AriaRole.Button, new() { Name = "Search", Exact = true });
    private readonly ILocator _issuesHeading = page.GetByRole(AriaRole.Heading, new() { Name = "Issues" });
    private readonly ILocator _openTabLink = page.GetByRole(AriaRole.Link, new() { Name = GitHubStateLabels.Open, Exact = false });
}
