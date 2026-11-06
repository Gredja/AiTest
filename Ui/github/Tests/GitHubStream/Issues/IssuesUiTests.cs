using Core.Generators;
using FluentAssertions;
using Ui.Github.Tests.Support;
using static Core.Helpers.TestDataConstants;

namespace Ui.Github.Tests.GitHubStream.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class IssuesUiTests : GitHubUiTestBase
{
    private readonly List<int> _createdIssueNumbers = [];

    [Test]
    [Category("Negative")]
    [Description("UI-4 Issues list: empty state on fresh sandbox, New issue available")]
    public async Task IssuesList_EmptyState_Verify()
    {
        await CloseAllOpenIssuesAsync();
        await Browser.IssuesPage.OpenAsync();

        var hasEmptyState = await Browser.IssuesPage.HasEmptyStateAsync();
        hasEmptyState.Should().BeTrue("fresh sandbox has no issues");

        var hasNewIssue = await Browser.IssuesPage.HasNewIssueButtonAsync();
        hasNewIssue.Should().BeTrue("authenticated user sees the New issue action");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-17 Issues search: keyword filter shows only the matching issue")]
    public async Task IssueSearch_FilterKeyword_ShowsOnlyMatch()
    {
        var keyword = $"zebrafilter{DataGenerator.RandomString(TitleRandomLength)}";
        var matchTitle = $"UI search {keyword}";
        var otherTitle = $"UI other {DataGenerator.RandomString(TitleRandomLength)}";
        await CreateIssueViaApiAsync(_createdIssueNumbers, isClosed: true, title: matchTitle);
        await CreateIssueViaApiAsync(_createdIssueNumbers, isClosed: true, title: otherTitle);

        await Browser.IssuesPage.OpenAsync();
        var search = await Browser.IssuesPage.SearchAsync(keyword);
        search.IsSuccess.Should().BeTrue(
            $"search must navigate to the filtered view (attempts={search.Attempts}, elapsed={search.Elapsed})");

        var found = await Browser.IssuesPage.WaitForIssueWithTitleAsync(matchTitle);
        found.IsSuccess.Should().BeTrue(
            $"matching issue must appear in the results (attempts={found.Attempts}, elapsed={found.Elapsed})");

        (await Browser.IssuesPage.HasIssueWithTitleAsync(otherTitle)).Should().BeFalse(
            "non-matching issue must not be in the filtered results");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-17 Issues list: Closed tab shows the API-closed issue")]
    public async Task IssueList_ClosedTab_ShowsApiClosed()
    {
        var title = $"UI closed {DataGenerator.RandomString(TitleRandomLength)}";
        await CreateIssueViaApiAsync(_createdIssueNumbers, isClosed: true, title: title);

        await Browser.IssuesPage.OpenAsync();
        var closedTab = await Browser.IssuesPage.ClickClosedTabAsync();
        closedTab.IsSuccess.Should().BeTrue(
            $"closed tab must filter the list (attempts={closedTab.Attempts}, elapsed={closedTab.Elapsed})");

        var found = await Browser.IssuesPage.WaitForIssueWithTitleAsync(title);
        found.IsSuccess.Should().BeTrue(
            $"closed issue must appear on the Closed tab (attempts={found.Attempts}, elapsed={found.Elapsed})");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-4 Issues list: repository Issues tab navigation lands on the list view")]
    public async Task Issues_NavigateViaTab_ShowsListView()
    {
        await Browser.RepositoryPage.OpenAsync();
        await Browser.RepositoryPage.NavigateToIssuesAsync();

        (await Browser.IssuesPage.HasIssuesHeadingAsync()).Should().BeTrue(
            "the Issues heading must render on the list view");
        (await Browser.IssuesPage.HasOpenTabAsync()).Should().BeTrue(
            "the Open tab counter must be part of the list view chrome");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var issueNumber in _createdIssueNumbers)
        {
            await Cleanup.CleanupIssueAsync(issueNumber);
        }
    }
}
