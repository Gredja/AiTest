using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Pages.GitHubStream.Issues;

internal sealed class IssueCreatePage(IPage page)
{
    // matches the fixture's action timeout (GitHubUiTestBase.UiTimeoutMs)
    private static readonly TimeSpan _openTimeout = TimeSpan.FromSeconds(15);

    private readonly IPage _page = page;

    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/issues/new");

    // this page is outside the load-verified list in Rules/ui-testing.md → Waits,
    // so the presence check polls instead of reading the count instantly
    internal async Task<WaitResult<bool>> WaitForOpenAsync() =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                var count = await _createIssueHeading.CountAsync();

                return count > 0;
            },
            isOpen => isOpen,
            _openTimeout);

    internal async Task CreateIssueAsync(string title, string description)
    {
        await _titleTextbox.FillAsync(title);
        await _descriptionTextbox.FillAsync(description);
        // accessible name includes the shortcut hint: "Create ( control enter )" — verified 2026-10-13
        await _createIssueButton.ClickAsync();

        // submit is a POST followed by a client-side redirect — wait for the heading that only exists
        // on the new issue page: rendering it implies the URL is already /issues/<number>
        await _issueHeadings.Filter(new() { HasTextString = title }).First.WaitForAsync();
    }

    private readonly ILocator _createIssueHeading = page.GetByRole(AriaRole.Heading, new() { Name = "Create new issue" });
    private readonly ILocator _titleTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Add a title" });
    private readonly ILocator _descriptionTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Markdown value" });
    private readonly ILocator _createIssueButton = page.GetByRole(AriaRole.Button, new() { Name = "Create ( control enter )" });
    private readonly ILocator _issueHeadings = page.GetByRole(AriaRole.Heading);
}
