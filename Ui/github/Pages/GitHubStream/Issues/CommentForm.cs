using Core.Helpers;
using Microsoft.Playwright;
using Ui.Github.Tests.Support;

namespace Ui.Github.Pages.GitHubStream.Issues;

internal sealed class CommentForm(IPage page)
{
    private readonly IPage _page = page;

    internal async Task AddCommentAsync(string body)
    {
        await _commentTextarea.FillAsync(body);
        await _commentButton.ClickAsync();

        // the comment is appended after the POST returns — wait for it before the caller asserts
        await _page.GetByText(body).First.WaitForAsync();
    }

    internal async Task<bool> HasCommentAsync(string body) =>
        await _page.GetByText(body).CountAsync() > 0;

    internal async Task EditFirstCommentAsync(string newBody)
    {
        await OpenActionsMenuAsync();
        await RetryHelper.UntilAsync(
            async () => await _editTextbox.CountAsync() > 0,
            () => _editMenuItem.ClickAsync());
        await _editTextbox.FillAsync(newBody);
        await _updateCommentButton.ClickAsync();

        // leaving edit mode implies the update POST returned; only then does the rendered
        // comment text (not the editor's content) prove the new body
        await _updateCommentButton.WaitForAsync(new() { State = WaitForSelectorState.Detached });
        await _page.GetByText(newBody).First.WaitForAsync();
    }

    internal async Task<WaitResult<bool>> DeleteFirstCommentAsync(string body)
    {
        await OpenActionsMenuAsync();
        await RetryHelper.UntilAsync(
            async () => await _deleteDialogHeading.CountAsync() > 0,
            () => _deleteMenuItem.ClickAsync());
        await _confirmDeleteButton.ClickAsync();

        return await WaitHelper.WaitUntilAsync(
            async () => await _page.GetByText(body).CountAsync() == 0,
            isGone => isGone);
    }

    // the trigger name carries a live timestamp ("Actions for {user}'s comment, {time}"),
    // so the comment-scoped menu is matched by its stable "Actions for" prefix; menu items
    // render before their handlers are wired — re-apply the trigger until the menu shows up
    private async Task OpenActionsMenuAsync() =>
        await RetryHelper.UntilAsync(
            async () => await _editMenuItem.CountAsync() > 0,
            () => _commentActionsButton.First.ClickAsync());

    private readonly ILocator _commentTextarea = page.GetByPlaceholder("Use Markdown to format your comment");
    private readonly ILocator _commentButton = page.GetByRole(AriaRole.Button, new() { Name = "Comment", Exact = true });
    private readonly ILocator _commentActionsButton = page.GetByRole(AriaRole.Button, new() { Name = "Actions for" });
    private readonly ILocator _editMenuItem = page.GetByRole(AriaRole.Menuitem, new() { Name = "Edit", Exact = true });
    private readonly ILocator _deleteMenuItem = page.GetByRole(AriaRole.Menuitem, new() { Name = "Delete", Exact = true });
    private readonly ILocator _editTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Markdown value" });
    private readonly ILocator _updateCommentButton = page.GetByRole(AriaRole.Button, new() { Name = "Update comment" });
    private readonly ILocator _deleteDialogHeading = page.GetByRole(AriaRole.Heading, new() { Name = "Delete comment" });
    private readonly ILocator _confirmDeleteButton = page.GetByRole(AriaRole.Button, new() { Name = "Delete", Exact = true });
}
