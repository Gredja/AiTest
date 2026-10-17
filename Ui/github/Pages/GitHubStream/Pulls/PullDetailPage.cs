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

    internal async Task<bool> ContainsTextAsync(string text) =>
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
    // close never reached the server) — re-apply until the outcome button appears.
    // A disabled button means the previous click is still in flight (form submit with
    // navigation) — clicking it would hang on actionability, so it is skipped, not clicked.
    internal async Task CloseAsync()
    {
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                if (await _reopenPullButton.CountAsync() > 0)
                {
                    return true;
                }

                if (await _closePullButton.CountAsync() > 0 && await _closePullButton.IsEnabledAsync())
                {
                    await _closePullButton.ClickAsync();
                }

                return false;
            },
            isClosed => isClosed,
            timeout: _actionTimeout,
            interval: _actionRetryInterval);

        await _closedStatusText.WaitForAsync();
    }

    internal async Task ReopenAsync()
    {
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                if (await _closePullButton.CountAsync() > 0)
                {
                    return true;
                }

                if (await _reopenPullButton.CountAsync() > 0 && await _reopenPullButton.IsEnabledAsync())
                {
                    await _reopenPullButton.ClickAsync();
                }

                return false;
            },
            isOpen => isOpen,
            timeout: _actionTimeout,
            interval: _actionRetryInterval);

        // the status badge re-renders after the button swap lands — wait for it too,
        // the test asserts on the badge, not the button
        await _openStatusText.WaitForAsync();
    }

    internal async Task BeginEditTitleAsync() =>
        await _editTitleButton.ClickAsync();

    internal async Task SaveTitleAsync(string newTitle)
    {
        await _editTitleTextbox.FillAsync(newTitle);
        await _saveButtonText.ClickAsync();
        await _pullHeadings.Filter(new() { HasTextString = newTitle }).First.WaitForAsync();
    }

    internal async Task EditBodyAsync(string newBody)
    {
        await _descriptionOptionsButton.ClickAsync();
        await _editCommentMenuItem.ClickAsync();
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

        return await WaitHelper.WaitUntilAsync(
            async () =>
            {
                await _page.ReloadAsync();
                return await _draftStatusText.CountAsync() > 0;
            },
            isDraft => isDraft,
            _actionTimeout);
    }

    // the editor hydrates after navigation — a fill before that lands the text visually
    // but leaves the submit disabled (observed 2026-10-17); re-fill until the button
    // enables, state check BEFORE every refill
    internal async Task AddCommentAsync(string body)
    {
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                if (await _commentButton.IsEnabledAsync())
                {
                    return true;
                }

                await _commentTextbox.FillAsync(body);
                return false;
            },
            isEnabled => isEnabled,
            timeout: _actionTimeout,
            interval: _actionRetryInterval);

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
