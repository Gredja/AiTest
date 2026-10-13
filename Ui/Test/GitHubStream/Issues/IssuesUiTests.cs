using FluentAssertions;
using AllureAdapter;
using Ui.Helper;

namespace Ui.Test.GitHubStream.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class IssuesUiTests : GitHubUiTestBase
{
    [Test]
    [Category("Regression")]
    [Description("UI-9 Issues list: empty state on fresh sandbox, New issue available")]
    public async Task IssuesList_EmptyState_Verify()
    {
        await Browser.IssuesPage.OpenAsync();

        var hasEmptyState = await Browser.IssuesPage.HasEmptyStateAsync();
        hasEmptyState.Should().BeTrue("fresh sandbox has no issues");

        var hasNewIssue = await Browser.IssuesPage.HasNewIssueButtonAsync();
        hasNewIssue.Should().BeTrue("authenticated user sees the New issue action");
    }
}
