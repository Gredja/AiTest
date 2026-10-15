using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Issues;

internal sealed class CommentForm
{
    private readonly IPage _page;

    internal CommentForm(IPage page) => _page = page;

    // textarea placeholder verified on issue #1 — 2026-10-13
    internal async Task AddCommentAsync(string body)
    {
        await _page.GetByPlaceholder("Use Markdown to format your comment").FillAsync(body);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Comment", Exact = true }).ClickAsync();

        // the comment is appended after the POST returns — wait for it before the caller asserts
        await _page.GetByText(body).First.WaitForAsync();
    }

    internal async Task<bool> ContainsCommentAsync(string body) =>
        await _page.GetByText(body).CountAsync() > 0;
}
