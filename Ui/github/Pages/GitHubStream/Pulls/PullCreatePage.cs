using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;
using Ui.Helpers;

namespace Ui.Github.Pages.GitHubStream.Pulls;

internal sealed class PullCreatePage(IPage page)
{
    internal async Task OpenCompareAsync(string baseBranch, string headBranch) =>
        await page.GotoAsync($"{TestConfig.UiBaseUrl}{GitHubUiEndpoints.ComparePath(baseBranch, headBranch)}");

    internal int? TryGetPullNumberFromUrl() => PageUrlHelper.TryGetResourceNumber(page.Url);

    internal async Task CreatePullRequestAsync(string title, string description)
    {
        await _createPullButton.ClickAsync();
        await _openPullHeading.WaitForAsync();
        await _titleTextbox.FillAsync(title);
        await _descriptionTextbox.FillAsync(description);
        await _createPullButton.ClickAsync();
        await page.WaitForURLAsync(url => url.Contains(GitHubUiEndpoints.PullFragment));

        // the PR heading renders after the redirect commits — it is the outcome of the submit
        await _pullHeadings.Filter(new() { HasTextString = title }).First.WaitForAsync();
    }

    private async Task<bool> HasCommitsSectionAsync() =>
        await _commitsHeadings.CountAsync() > 0;

    // the compare view is not server-rendered — the commits section may appear after load
    internal async Task<WaitResult<bool>> WaitForCommitsSectionAsync() =>
        await WaitHelper.WaitUntilAsync(() => HasCommitsSectionAsync(), hasCommits => hasCommits);

    private readonly ILocator _createPullButton = page.GetByRole(AriaRole.Button, new() { Name = "Create pull request" });
    private readonly ILocator _openPullHeading = page.GetByRole(AriaRole.Heading, new() { Name = "Open a pull request" });
    private readonly ILocator _titleTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Add a title *" });
    private readonly ILocator _descriptionTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Comment" });
    private readonly ILocator _pullHeadings = page.GetByRole(AriaRole.Heading);
    private readonly ILocator _commitsHeadings = page.GetByRole(AriaRole.Heading).Filter(new() { HasTextString = "Commits on" });
}
