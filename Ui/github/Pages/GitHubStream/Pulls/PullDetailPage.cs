using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;
using Ui.Github.Tests.Support;

namespace Ui.Github.Pages.GitHubStream.Pulls;

internal sealed class PullDetailPage(IPage page)
{
    private static readonly TimeSpan _actionTimeout =
        TimeSpan.FromMilliseconds(GitHubUiTestBase.UiTimeoutMs);

    // must exceed the ~300ms render latency of GitHub controls, or the retry
    // re-applies before the previous effect lands
    private static readonly TimeSpan _actionRetryInterval = TimeSpan.FromMilliseconds(500);

    private readonly IPage _page = page;

    internal async Task OpenAsync(int pullNumber) =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/pull/{pullNumber}");

    internal int? TryGetPullNumberFromUrl()
    {
        var lastSegment = _page.Url.Split('?', '#')[0].TrimEnd('/').Split('/').Last();

        return int.TryParse(lastSegment, out var pullNumber) ? pullNumber : null;
    }

    internal async Task<bool> HasTitleAsync(string title) =>
        await _pullHeadings.Filter(new() { HasTextString = title }).CountAsync() > 0;

    internal async Task<bool> HasBranchLinkAsync(string branchName) =>
        await _branchLinks.Filter(new() { HasTextString = branchName }).CountAsync() > 0;

    internal async Task<bool> HasTextAsync(string text) =>
        await _page.GetByText(text).CountAsync() > 0;

    internal async Task OpenFilesChangedAsync(int pullNumber) =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/pull/{pullNumber}/files");

    // the diff view hydrates after navigation — poll presence instead of an instant count
    internal async Task<WaitResult<bool>> WaitForFileAsync(string fileName) =>
        await WaitHelper.WaitUntilAsync(
            async () => await _fileTreeItems.Filter(new() { HasTextString = fileName }).CountAsync() > 0,
            isPresent => isPresent);

    internal async Task<bool> HasOpenStatusAsync() =>
        await _openStatusText.CountAsync() > 0;

    internal async Task<bool> HasClosedStatusAsync() =>
        await _closedStatusText.CountAsync() > 0;

    // the action click can be swallowed while the page hydrates (observed 2026-10-17:
    // close never reached the server) — re-apply until the outcome button appears
    internal async Task CloseAsync()
    {
        await RetryHelper.UntilAsync(
            async () => await _reopenPullButton.CountAsync() > 0,
            () => ClickOrReloadAsync(_closePullButton),
            _actionTimeout,
            _actionRetryInterval);

        await _closedStatusText.WaitForAsync();
    }

    internal async Task ReopenAsync()
    {
        await RetryHelper.UntilAsync(
            async () => await _closePullButton.CountAsync() > 0,
            () => ClickOrReloadAsync(_reopenPullButton),
            _actionTimeout,
            _actionRetryInterval);

        // the status badge re-renders after the button swap lands — wait for it too,
        // the test asserts on the badge, not the button
        await _openStatusText.WaitForAsync();
    }

    // no action button at all = the page is mid-rerender or served stale against the
    // API write → reload-poll; a disabled button means the previous click is still in
    // flight — clicking it would hang on actionability, so it is skipped
    private async Task ClickOrReloadAsync(ILocator actionButton)
    {
        if (await actionButton.CountAsync() == 0)
        {
            await _page.ReloadAsync();

            return;
        }

        if (await actionButton.IsEnabledAsync())
        {
            await actionButton.ClickAsync();
        }
    }

    // the edit-title click can be swallowed while the page hydrates (observed 2026-10-19:
    // the editor never opened) — re-apply until the editor textbox shows up
    internal async Task BeginEditTitleAsync() =>
        await RetryHelper.UntilAsync(
            async () => await _editTitleTextbox.CountAsync() > 0,
            () => _editTitleButton.ClickAsync(),
            _actionTimeout,
            _actionRetryInterval);

    internal async Task SaveTitleAsync(string newTitle)
    {
        await _editTitleTextbox.FillAsync(newTitle);
        await _saveButtonText.ClickAsync();
        await _pullHeadings.Filter(new() { HasTextString = newTitle }).First.WaitForAsync();
    }

    internal async Task EditBodyAsync(string newBody)
    {
        // the options menu lazy-loads its items over HTTP — click the trigger only while the
        // menu is closed, click the item once it attached
        await RetryHelper.UntilAsync(
            async () => await _commentBodyTextbox.CountAsync() > 0,
            async () =>
            {
                if (await _editCommentMenuItem.CountAsync() > 0)
                {
                    await _editCommentMenuItem.ClickAsync();
                }
                else
                {
                    await _descriptionOptionsButton.ClickAsync();
                }
            },
            _actionTimeout,
            _actionRetryInterval);

        await _commentBodyTextbox.FillAsync(newBody);
        await _updateCommentButton.ClickAsync();
        await _page.GetByText(newBody).First.WaitForAsync();
    }

    // The confirm POST can return 200 while the client leaves the dialog open and the
    // badge stale (observed 2026-10-17: draft accepted server-side, UI unchanged) —
    // await the response, then poll the badge via reload; the server is the source of truth
    internal async Task<WaitResult<bool>> ConvertToDraftAsync()
    {
        var convertResponse = _page.WaitForResponseAsync(response => response.Url.Contains("convert_to_draft"));
        await _convertToDraftButton.ClickAsync();
        await _draftConfirmButton.ClickAsync();
        await convertResponse;

        return await RetryHelper.UntilAsync(
            async () => await _draftStatusText.CountAsync() > 0,
            () => _page.ReloadAsync(),
            _actionTimeout);
    }

    // the editor hydrates after navigation — a fill before that lands the text visually
    // but leaves the submit disabled (observed 2026-10-17); re-fill until the button enables
    internal async Task AddCommentAsync(string body)
    {
        await RetryHelper.UntilAsync(
            async () => await _commentButton.IsEnabledAsync(),
            () => _commentTextbox.FillAsync(body),
            _actionTimeout,
            _actionRetryInterval);

        await _commentButton.ClickAsync();
        await _page.GetByText(body).First.WaitForAsync();
    }

    private readonly ILocator _pullHeadings = page.GetByRole(AriaRole.Heading);
    private readonly ILocator _branchLinks = page.GetByRole(AriaRole.Link);
    private readonly ILocator _fileTreeItems = page.GetByRole(AriaRole.Treeitem);
    private readonly ILocator _openStatusText = page.GetByText("Open", new() { Exact = true }).First;
    private readonly ILocator _closedStatusText = page.GetByText("Closed", new() { Exact = true }).First;
    private readonly ILocator _draftStatusText = page.GetByText("Draft", new() { Exact = true }).First;
    private readonly ILocator _closePullButton = page.GetByRole(AriaRole.Button, new() { Name = "Close pull request" });
    private readonly ILocator _reopenPullButton = page.GetByRole(AriaRole.Button, new() { Name = "Reopen pull request" });
    private readonly ILocator _editTitleButton = page.GetByRole(AriaRole.Button, new() { Name = "Edit title" });
    private readonly ILocator _editTitleTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Edit Pull Request Title" });
    private readonly ILocator _saveButtonText = page.GetByText("Save", new() { Exact = true });
    private readonly ILocator _descriptionOptionsButton = page.GetByRole(AriaRole.Button, new() { Name = "Show options" }).First;
    private readonly ILocator _editCommentMenuItem = page.GetByRole(AriaRole.Menuitem, new() { Name = "Edit comment" });
    private readonly ILocator _commentBodyTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Comment body" });
    private readonly ILocator _updateCommentButton = page.GetByRole(AriaRole.Button, new() { Name = "Update comment" });
    // two triggers on the page carry this name (sidebar button + summary) — strict risk,
    // .First picks one deterministically; either opens a confirm dialog
    private readonly ILocator _convertToDraftButton = page.GetByRole(AriaRole.Button, new() { Name = "Convert to draft" }).First;

    // GitHub renders the confirm either as role=alertdialog (primer portal) or
    // role=dialog (details-overlay) — cover both
    private readonly ILocator _draftConfirmButton = page
        .GetByRole(AriaRole.Dialog)
        .Or(page.GetByRole(AriaRole.Alertdialog))
        .GetByRole(AriaRole.Button, new() { Name = "Convert to draft" });
    private readonly ILocator _commentTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Comment" });
    private readonly ILocator _commentButton = page.GetByRole(AriaRole.Button, new() { Name = "Comment", Exact = true });
}
