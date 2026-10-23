using Core.Config;
using Microsoft.Playwright;
using Ui.Helpers;

namespace Ui.Github.Pages.GitHubStream.Pulls;

internal sealed class PullCreatePage(IPage page)
{
    private readonly IPage _page = page;

    internal async Task OpenCompareAsync(string baseBranch, string headBranch) =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/compare/{baseBranch}...{headBranch}");

    internal int? TryGetPullNumberFromUrl() => PageUrlHelper.TryGetResourceNumber(_page.Url);

    internal async Task CreatePullRequestAsync(string title, string description)
    {
        await _createPullButton.ClickAsync();
        await _openPullHeading.WaitForAsync();
        await _titleTextbox.FillAsync(title);
        await _descriptionTextbox.FillAsync(description);
        await _createPullButton.ClickAsync();
        await _page.WaitForURLAsync(url => url.Contains("/pull/"));

        // the PR heading renders after the redirect commits — it is the outcome of the submit
        await _pullHeadings.Filter(new() { HasTextString = title }).First.WaitForAsync();
    }

    internal async Task<bool> HasCommitsSectionAsync() =>
        await _commitsHeadings.CountAsync() > 0;

    private readonly ILocator _createPullButton = page.GetByRole(AriaRole.Button, new() { Name = "Create pull request" });
    private readonly ILocator _openPullHeading = page.GetByRole(AriaRole.Heading, new() { Name = "Open a pull request" });
    private readonly ILocator _titleTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Add a title *" });
    private readonly ILocator _descriptionTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Comment" });
    private readonly ILocator _pullHeadings = page.GetByRole(AriaRole.Heading);
    private readonly ILocator _commitsHeadings = page.GetByRole(AriaRole.Heading).Filter(new() { HasTextString = "Commits on" });
}
