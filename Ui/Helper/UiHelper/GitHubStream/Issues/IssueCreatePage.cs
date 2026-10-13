using Core.Config;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Issues;

internal sealed class IssueCreatePage
{
    private readonly IPage _page;

    internal IssueCreatePage(IPage page) => _page = page;

    internal async Task OpenAsync()
    {
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/issues/new");
    }

    internal async Task<bool> IsOpenAsync() =>
        await _page.GetByRole(AriaRole.Heading, new() { Name = "Create new issue" }).CountAsync() > 0;

    internal async Task CreateIssueAsync(string title, string description)
    {
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Add a title" }).FillAsync(title);
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Markdown value" }).FillAsync(description);
        // accessible name includes the shortcut hint: "Create ( control enter )" — verified 2026-10-13
        await _page.GetByRole(AriaRole.Button, new() { Name = "Create ( control enter )" }).ClickAsync();
    }
}
