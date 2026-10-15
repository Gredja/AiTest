using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Issues;

internal sealed class IssueCreatePage
{
    // matches the fixture's action timeout (GitHubUiTestBase.UiTimeoutMs)
    private static readonly TimeSpan _openTimeout = TimeSpan.FromSeconds(15);

    private readonly IPage _page;

    internal IssueCreatePage(IPage page) => _page = page;

    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/issues/new");

    // this page is outside the load-verified list in Rules/ui-testing.md → Waits,
    // so the presence check polls instead of reading the count instantly
    internal async Task<WaitResult<bool>> WaitForOpenAsync() =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                var count = await _page.GetByRole(AriaRole.Heading, new() { Name = "Create new issue" }).CountAsync();

                return count > 0;
            },
            isOpen => isOpen,
            _openTimeout);

    internal async Task CreateIssueAsync(string title, string description)
    {
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Add a title" }).FillAsync(title);
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Markdown value" }).FillAsync(description);
        // accessible name includes the shortcut hint: "Create ( control enter )" — verified 2026-10-13
        await _page.GetByRole(AriaRole.Button, new() { Name = "Create ( control enter )" }).ClickAsync();

        // submit is a POST followed by a client-side redirect — wait for the heading that only exists
        // on the new issue page: rendering it implies the URL is already /issues/<number>
        await _page.GetByRole(AriaRole.Heading).Filter(new() { HasTextString = title }).First.WaitForAsync();
    }
}
