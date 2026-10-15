using Core.Config;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Issues;

internal sealed class IssueDetailPage
{
    private readonly IPage _page;

    internal IssueDetailPage(IPage page) => _page = page;

    internal async Task OpenAsync(int issueNumber) =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/issues/{issueNumber}");

    internal int? TryGetIssueNumberFromUrl()
    {
        var lastSegment = _page.Url.Split('?', '#')[0].TrimEnd('/').Split('/').Last();

        return int.TryParse(lastSegment, out var issueNumber) ? issueNumber : null;
    }

    internal async Task<bool> HasTitleAsync(string title) =>
        await _page.GetByRole(AriaRole.Heading).Filter(new() { HasTextString = title }).CountAsync() > 0;

    // Status badge near the top: "Open" (img Issue + text) / "Closed" — verified 2026-10-13
    internal async Task<bool> HasClosedStatusAsync() =>
        await _page.GetByText("Closed", new() { Exact = true }).First.CountAsync() > 0;

    internal async Task<bool> HasOpenStatusAsync() =>
        await _page.GetByText("Open", new() { Exact = true }).First.CountAsync() > 0;

    // Buttons verified via DOM on issue #1 — 2026-10-13
    // State flips after a network round-trip; the swap to the opposite button is the
    // unambiguous post-condition (the status text may already be on the page)
    internal async Task CloseAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Close issue" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Reopen issue" }).WaitForAsync();
    }

    internal async Task ReopenAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Reopen issue" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Close issue" }).WaitForAsync();
    }

    internal async Task BeginEditTitleAsync() =>
        await _page.GetByRole(AriaRole.Button, new() { Name = "Edit issue title" }).ClickAsync();

    internal async Task SaveTitleAsync(string newTitle)
    {
        await _page.GetByRole(AriaRole.Textbox).First.FillAsync(newTitle);

        // the button's accessible name carries an sr-only shortcut hint ("Save ( enter )"), so a
        // role+name match never hits; the label span itself is exactly "Save"
        await _page.GetByText("Save", new() { Exact = true }).ClickAsync();
        await _page.GetByRole(AriaRole.Heading).Filter(new() { HasTextString = newTitle }).First.WaitForAsync();
    }
}
