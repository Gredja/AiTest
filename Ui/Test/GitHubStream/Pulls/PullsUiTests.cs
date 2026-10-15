using FluentAssertions;
using AllureAdapter;
using Ui.Helper;

namespace Ui.Test.GitHubStream.Pulls;

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
}
