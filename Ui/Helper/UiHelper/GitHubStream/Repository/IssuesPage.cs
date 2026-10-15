using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Repository;

internal sealed class IssuesPage
{
    private readonly IPage _page;

    internal IssuesPage(IPage page) => _page = page;

    internal async Task OpenAsync()
    {
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/issues");
    }

    internal async Task<bool> HasEmptyStateAsync() =>
        await _page.GetByRole(AriaRole.Heading, new() { Name = "No results matched your search" }).CountAsync() > 0;

    internal async Task<bool> HasNewIssueButtonAsync() =>
        await _page.GetByRole(AriaRole.Link, new() { Name = "New issue" }).CountAsync() > 0;

    // Tab counter renders as "Open (0)" (anonymous) or "Open 0" (authenticated) — same pattern as PullsPage
    internal async Task<int> GetOpenCountAsync()
    {
        var text = await _page.GetByRole(AriaRole.Link)
            .Filter(new() { HasTextString = "Open" })
            .First.TextContentAsync();
        var match = System.Text.RegularExpressions.Regex.Match(text ?? string.Empty, @"Open\D*(\d+)");

        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
    }

    internal async Task<bool> ContainsIssueWithTitleAsync(string title) =>
        await _page.GetByRole(AriaRole.Link).Filter(new() { HasTextString = title }).CountAsync() > 0;

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
}
