using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using Microsoft.Playwright;
using Ui.Helpers;

namespace Ui.Github.Pages.GitHubStream.Repository;

internal sealed class CodePage(IPage page) : RepositoryPageBase(page)
{
    private static readonly TimeSpan _presenceTimeout =
        TimeSpan.FromMilliseconds(UiTimeout.UiTimeoutMs);

    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/tree/main");

    internal async Task OpenFolderAsync(string branchName, string folderName) =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/tree/{branchName}/{folderName}");

    internal async Task<bool> HasFileAsync(string fileName) =>
        await _fileLinks.Filter(new() { HasTextString = fileName }).CountAsync() > 0;

    internal async Task OpenReadmeFileAsync() =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/blob/main/README.md");

    internal async Task<bool> HasRenderedReadmeContentAsync()
    {
        // The blob page renders markdown after navigation (and GitHub now shows a
        // Preview/Code toggle) — the outcome must be awaited, never sampled once
        var wait = await WaitHelper.WaitUntilAsync(
            () => _renderedReadmeHeading.CountAsync(),
            count => count > 0);
        return wait.IsSuccess;
    }

    internal async Task OpenCommitsAsync() =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/commits");

    internal async Task<WaitResult<bool>> WaitForCommitAsync(string message) =>
        await WaitHelper.WaitUntilAsync(
            async () => await _commitHeadings.Filter(new() { HasTextString = message }).CountAsync() > 0,
            found => found,
            _presenceTimeout);

    private readonly ILocator _fileLinks = page.GetByRole(AriaRole.Link);
    // README H1 is the repo's short name ("# ui-sandbox"), not the full slug —
    // Ui.TestRepo is "gredjapl/ui-sandbox" (observed mismatch 2026-11-09)
    private readonly ILocator _renderedReadmeHeading = page.GetByRole(AriaRole.Heading, new() { Name = GitHubParamHelper.ParseRepo(TestConfig.UiTestRepo).Repo });
    private readonly ILocator _commitHeadings = page.GetByRole(AriaRole.Heading);
}
