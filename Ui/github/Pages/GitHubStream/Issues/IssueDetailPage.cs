using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;
using Ui.Helpers;

namespace Ui.Github.Pages.GitHubStream.Issues;

internal sealed class IssueDetailPage(IPage page)
{
    internal async Task OpenAsync(int issueNumber) =>
        await page.GotoAsync($"{TestConfig.UiBaseUrl}{GitHubUiEndpoints.IssuePath(issueNumber)}");

    internal int? TryGetIssueNumberFromUrl() => PageUrlHelper.TryGetResourceNumber(page.Url);

    internal async Task<bool> HasTitleAsync(string title) =>
        await _issueHeadings.Filter(new() { HasTextString = title }).CountAsync() > 0;

    internal async Task<bool> HasClosedStatusAsync() =>
        await _closedStatusText.CountAsync() > 0;

    internal async Task<bool> HasOpenStatusAsync() =>
        await _openStatusText.CountAsync() > 0;

    // State flips after a network round-trip; wait for the outcome button first (unambiguous),
    // then for the status badge the test asserts on — it re-renders after the swap lands
    internal async Task CloseAsync()
    {
        await _closeIssueButton.ClickAsync();
        await _reopenIssueButton.WaitForAsync();
        await _closedStatusText.WaitForAsync();
    }

    internal async Task ReopenAsync()
    {
        await _reopenIssueButton.ClickAsync();
        await _closeIssueButton.WaitForAsync();
        await _openStatusText.WaitForAsync();
    }

    // the edit-title click can be swallowed while the page hydrates (observed 2026-10-19 and
    // 2026-11-12: the editor never opened and the fill landed in the comment box) — the title
    // textarea is ambiguous (the always-visible comment box matches Textbox.First), so the
    // Save button is the unambiguous editor-open post-condition; state check first, so a late
    // render is never toggled shut
    internal async Task BeginEditTitleAsync()
    {
        var result = await RetryHelper.UntilAsync(
            async () => await _saveButtonText.CountAsync() > 0,
            () => _editIssueTitleButton.ClickAsync());

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"title editor did not open (attempts={result.Attempts}, elapsed={result.Elapsed})");
        }
    }

    internal async Task SaveTitleAsync(string newTitle)
    {
        await _firstTextbox.FillAsync(newTitle);

        // the button's accessible name carries an sr-only shortcut hint ("Save ( enter )"), so a
        // role+name match never hits; the label span itself is exactly "Save"
        await _saveButtonText.ClickAsync();
        await _issueHeadings.Filter(new() { HasTextString = newTitle }).First.WaitForAsync();
    }

    // the gear click can be swallowed before its React handler is wired (observed 2026-11-12:
    // the picker never opened) — re-apply until the options render; state check first, so an
    // already-open picker is never toggled shut
    internal async Task OpenLabelsPickerAsync()
    {
        // exact case-sensitive name: the sidebar also has an "Edit labels" link
        var result = await RetryHelper.UntilAsync(
            async () => await _labelOptions.CountAsync() > 0,
            () => _editLabelsButton.ClickAsync());

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"labels picker did not open (attempts={result.Attempts}, elapsed={result.Elapsed})");
        }
    }

    // the option renders before its React handler is wired — the first click can be swallowed
    // silently (same hydration gap as the branches delete), so re-apply until selected flips
    internal Task<WaitResult<bool>> SelectLabelAsync(string label) =>
        ClickLabelOptionUntilAsync(label, shouldBeSelected: true);

    internal Task<WaitResult<bool>> DeselectLabelAsync(string label) =>
        ClickLabelOptionUntilAsync(label, shouldBeSelected: false);

    internal async Task CloseLabelsPickerAsync()
    {
        await _editLabelsButton.ClickAsync();
        await _labelOptions.First.WaitForAsync(new() { State = WaitForSelectorState.Detached });
    }

    internal async Task<bool> HasLabelChipAsync(string label) =>
        await _labelLinks.Filter(new() { HasTextString = label }).CountAsync() > 0;

    private async Task<WaitResult<bool>> ClickLabelOptionUntilAsync(string label, bool shouldBeSelected) =>
        await RetryHelper.UntilAsync(
            async () => (await page
                .GetByRole(AriaRole.Option, new() { Name = label, Selected = true })
                .CountAsync() > 0) == shouldBeSelected,
            () => _labelOptions.Filter(new() { HasTextString = label }).First.ClickAsync());

    private readonly ILocator _issueHeadings = page.GetByRole(AriaRole.Heading);
    private readonly ILocator _closedStatusText = page.GetByText(GitHubStateLabels.Closed, new() { Exact = true }).First;
    private readonly ILocator _openStatusText = page.GetByText(GitHubStateLabels.Open, new() { Exact = true }).First;
    private readonly ILocator _closeIssueButton = page.GetByRole(AriaRole.Button, new() { Name = "Close issue" });
    private readonly ILocator _reopenIssueButton = page.GetByRole(AriaRole.Button, new() { Name = "Reopen issue" });
    private readonly ILocator _editIssueTitleButton = page.GetByRole(AriaRole.Button, new() { Name = "Edit issue title" });
    private readonly ILocator _firstTextbox = page.GetByRole(AriaRole.Textbox).First;
    private readonly ILocator _saveButtonText = page.GetByText("Save", new() { Exact = true });
    private readonly ILocator _editLabelsButton = page.GetByRole(AriaRole.Button, new() { Name = "Edit Labels", Exact = true });
    private readonly ILocator _labelOptions = page.GetByRole(AriaRole.Option);
    private readonly ILocator _labelLinks = page.GetByRole(AriaRole.Link);
}
