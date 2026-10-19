using FluentAssertions;
using AllureAdapter;
using Ui.Github.Tests.Support;

namespace Ui.Github.Tests.GitHubStream.Pulls;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class PullsUiTests : GitHubUiTestBase
{
    [Test]
    [Category("Regression")]
    [Description("UI-7 Pull requests list: empty state on fresh sandbox")]
    public async Task PullsList_EmptyState_Verify()
    {
        await Browser.PullsPage.OpenAsync();

        var hasEmptyState = await Browser.PullsPage.HasEmptyStateAsync();
        hasEmptyState.Should().BeTrue("fresh sandbox has no pull requests");

        var openCount = await Browser.PullsPage.GetOpenCountAsync();
        openCount.Should().Be(0, "open tab must show zero PRs");
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
