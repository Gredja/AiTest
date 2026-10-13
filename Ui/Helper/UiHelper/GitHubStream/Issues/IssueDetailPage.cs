using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Issues;

internal sealed class IssueDetailPage
{
    private readonly IPage _page;

    internal IssueDetailPage(IPage page) => _page = page;

    internal string CurrentUrl => _page.Url;

    internal async Task<bool> HasTitleAsync(string title) =>
        await _page.GetByRole(AriaRole.Heading).Filter(new() { HasTextString = title }).CountAsync() > 0;

    internal async Task<bool> HasClosedBadgeAsync() =>
        await _page.GetByText("Closed", new() { Exact = true }).CountAsync() > 0;

    internal async Task<bool> HasOpenBadgeAsync() =>
        await _page.GetByText("Open", new() { Exact = true }).CountAsync() > 0;

    internal async Task CloseAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Close issue" }).ClickAsync();
    }

    internal async Task ReopenAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Reopen issue" }).ClickAsync();
    }
}
