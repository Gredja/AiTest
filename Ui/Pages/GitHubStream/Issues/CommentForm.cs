using Microsoft.Playwright;

namespace Ui.Pages.GitHubStream.Issues;

internal sealed class CommentForm(IPage page)
{
    private readonly IPage _page = page;

    // textarea placeholder verified on issue #1 — 2026-10-13
    internal async Task AddCommentAsync(string body)
    {
        await _commentTextarea.FillAsync(body);
        await _commentButton.ClickAsync();

        // the comment is appended after the POST returns — wait for it before the caller asserts
        await _page.GetByText(body).First.WaitForAsync();
    }

    internal async Task<bool> ContainsCommentAsync(string body) =>
        await _page.GetByText(body).CountAsync() > 0;

    private readonly ILocator _commentTextarea = page.GetByPlaceholder("Use Markdown to format your comment");
    private readonly ILocator _commentButton = page.GetByRole(AriaRole.Button, new() { Name = "Comment", Exact = true });
}
