using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;
using Ui.Helper;

namespace Ui.Pages.GitHubStream.Repository;

internal sealed class IssuesPage(IPage page)
{
    private readonly IPage _page = page;

    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/issues");

    internal async Task<bool> HasEmptyStateAsync() =>
        await _emptyStateHeading.CountAsync() > 0;

    internal async Task<bool> HasNewIssueButtonAsync() =>
        await _newIssueLink.CountAsync() > 0;

    internal async Task<int> GetOpenCountAsync()
    {
        var text = await _openTabLink.TextContentAsync();

        return TabCountParser.Parse(text);
    }

    internal async Task<bool> ContainsIssueWithTitleAsync(string title) =>
        await _issueLinks.Filter(new() { HasTextString = title }).CountAsync() > 0;

    // The list is indexed asynchronously after a write: the page loaded right after a create
    // can still miss the new issue (verified 2026-10-14) — poll with a reload instead of
    // a single instant count, per Rules/test-practices.md read-after-write
    internal async Task<WaitResult<bool>> WaitForIssueWithTitleAsync(string title) =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                await _page.ReloadAsync();

                return await ContainsIssueWithTitleAsync(title);
            },
            found => found);

    private readonly ILocator _emptyStateHeading = page.GetByRole(AriaRole.Heading, new() { Name = "No results matched your search" });
    private readonly ILocator _newIssueLink = page.GetByRole(AriaRole.Link, new() { Name = "New issue" });
    private readonly ILocator _issueLinks = page.GetByRole(AriaRole.Link);
    private readonly ILocator _openTabLink = page.GetByRole(AriaRole.Link).Filter(new() { HasTextString = "Open" }).First;
}
