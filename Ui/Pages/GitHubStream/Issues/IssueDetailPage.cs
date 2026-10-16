using Core.Config;
using Microsoft.Playwright;

namespace Ui.Pages.GitHubStream.Issues;

internal sealed class IssueDetailPage(IPage page)
{
    private readonly IPage _page = page;

    internal async Task OpenAsync(int issueNumber) =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/issues/{issueNumber}");

    internal int? TryGetIssueNumberFromUrl()
    {
        var lastSegment = _page.Url.Split('?', '#')[0].TrimEnd('/').Split('/').Last();

        return int.TryParse(lastSegment, out var issueNumber) ? issueNumber : null;
    }

    internal async Task<bool> HasTitleAsync(string title) =>
        await _issueHeadings.Filter(new() { HasTextString = title }).CountAsync() > 0;

    // Status badge near the top: "Open" (img Issue + text) / "Closed" — verified 2026-10-13
    internal async Task<bool> HasClosedStatusAsync() =>
        await _closedStatusText.CountAsync() > 0;

    internal async Task<bool> HasOpenStatusAsync() =>
        await _openStatusText.CountAsync() > 0;

    // Buttons verified via DOM on issue #1 — 2026-10-13
    // State flips after a network round-trip; the swap to the opposite button is the
    // unambiguous post-condition (the status text may already be on the page)
    internal async Task CloseAsync()
    {
        await _closeIssueButton.ClickAsync();
        await _reopenIssueButton.WaitForAsync();
    }

    internal async Task ReopenAsync()
    {
        await _reopenIssueButton.ClickAsync();
        await _closeIssueButton.WaitForAsync();
    }

    internal async Task BeginEditTitleAsync() =>
        await _editIssueTitleButton.ClickAsync();

    internal async Task SaveTitleAsync(string newTitle)
    {
        await _firstTextbox.FillAsync(newTitle);

        // the button's accessible name carries an sr-only shortcut hint ("Save ( enter )"), so a
        // role+name match never hits; the label span itself is exactly "Save"
        await _saveButtonText.ClickAsync();
        await _issueHeadings.Filter(new() { HasTextString = newTitle }).First.WaitForAsync();
    }

    private readonly ILocator _issueHeadings = page.GetByRole(AriaRole.Heading);
    private readonly ILocator _closedStatusText = page.GetByText("Closed", new() { Exact = true }).First;
    private readonly ILocator _openStatusText = page.GetByText("Open", new() { Exact = true }).First;
    private readonly ILocator _closeIssueButton = page.GetByRole(AriaRole.Button, new() { Name = "Close issue" });
    private readonly ILocator _reopenIssueButton = page.GetByRole(AriaRole.Button, new() { Name = "Reopen issue" });
    private readonly ILocator _editIssueTitleButton = page.GetByRole(AriaRole.Button, new() { Name = "Edit issue title" });
    private readonly ILocator _firstTextbox = page.GetByRole(AriaRole.Textbox).First;
    private readonly ILocator _saveButtonText = page.GetByText("Save", new() { Exact = true });
}
