using FluentAssertions;
using Ui.Github.Tests.Support;

namespace Ui.Github.Tests.GitHubStream.Pulls;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class PullsUiTests : GitHubUiTestBase
{
    [Test]
    [Category("Negative")]
    [Description("UI-7 Pull requests list: empty state on fresh sandbox")]
    public async Task PullsList_EmptyState_Verify()
    {
        await CloseAllOpenPullRequestsAsync();
        await Browser.PullsPage.OpenAsync();

        var emptyState = await Browser.PullsPage.WaitForEmptyStateAsync();
        emptyState.IsSuccess.Should().BeTrue(
            $"fresh sandbox has no pull requests (attempts={emptyState.Attempts}, elapsed={emptyState.Elapsed})");

        var openCount = await Browser.PullsPage.WaitForOpenCountAsync(0);
        openCount.IsSuccess.Should().BeTrue(
            $"open tab must show zero PRs (attempts={openCount.Attempts}, elapsed={openCount.Elapsed}, last={openCount.LastValue})");
    }

    [Test]
    [Category("Negative")]
    [Description("UI-23 Non-existent pull request: 404 illustration shown")]
    public async Task PullNotFound_ViaUI_Shows404()
    {
        var nonExistentNumber = await ResolveNonExistentPullNumberAsync();
        await Browser.PullDetailPage.OpenAsync(nonExistentNumber);

        var notFound = await Browser.NotFoundPage.WaitFor404MessageAsync();
        notFound.IsSuccess.Should().BeTrue(
            $"GitHub serves the 404 illustration for missing pull requests (attempts={notFound.Attempts}, elapsed={notFound.Elapsed})");
    }
}
