using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Issues;

internal sealed class CommentForm
{
    private readonly IPage _page;

    internal CommentForm(IPage page) => _page = page;

    internal async Task AddCommentAsync(string body)
    {
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Markdown value" }).Last.FillAsync(body);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Comment", Exact = true }).ClickAsync();
    }

    internal async Task<bool> ContainsCommentAsync(string body) =>
        await _page.GetByText(body, new() { Exact = true }).CountAsync() > 0;
}
