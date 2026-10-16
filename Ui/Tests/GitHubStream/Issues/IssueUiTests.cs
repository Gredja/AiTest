using Core.Helpers;
using FluentAssertions;
using AllureAdapter;
using Ui.Tests.Support;
using static Core.Helpers.TestDataConstants;

namespace Ui.Tests.GitHubStream.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class IssueUiTests : GitHubUiTestBase
{
    private readonly List<int> _createdIssueNumbers = [];
    private readonly List<long> _createdCommentIds = [];

    [Test]
    [Category("Regression")]
    [Description("UI-5 Issue create via UI: appears in list, registered for cleanup")]
    public async Task IssueCreate_ViaUI_AppearsInList()
    {
        var title = $"UI create {DataGenerator.RandomString(TitleRandomLength)}";

        await Browser.IssueCreatePage.OpenAsync();
        var opened = await Browser.IssueCreatePage.WaitForOpenAsync();
        opened.IsSuccess.Should().BeTrue(
            $"create form must open (attempts={opened.Attempts}, elapsed={opened.Elapsed})");
        await Browser.IssueCreatePage.CreateIssueAsync(title, "Created through the UI create form");

        var issueNumber = Browser.IssueDetailPage.TryGetIssueNumberFromUrl();
        issueNumber.Should().NotBeNull("submit must redirect to the issue page");
        _createdIssueNumbers.Add(issueNumber!.Value);

        (await Browser.IssueDetailPage.HasTitleAsync(title)).Should().BeTrue("issue page must show the title");

        await Browser.IssuesPage.OpenAsync();
        var found = await Browser.IssuesPage.WaitForIssueWithTitleAsync(title);
        found.IsSuccess.Should().BeTrue(
            $"issue must appear in the list (attempts={found.Attempts}, elapsed={found.Elapsed})");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-6 Issue close via UI: shows Closed status")]
    public async Task IssueClose_ViaUI_ShowsClosedBadge()
    {
        var issueNumber = await CreateIssueViaApiAsync(_createdIssueNumbers);
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.IssueDetailPage.CloseAsync();

        (await Browser.IssueDetailPage.HasClosedStatusAsync()).Should().BeTrue("status must flip to Closed");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-6 Comment create via UI: visible in thread")]
    public async Task CommentCreate_ViaUI_IsVisible()
    {
        var body = $"UI comment {DataGenerator.RandomString(TitleRandomLength)}";
        var issueNumber = await CreateIssueViaApiAsync(_createdIssueNumbers);
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.CommentForm.AddCommentAsync(body);
        await RegisterCommentIdAsync(issueNumber, body, _createdCommentIds);

        (await Browser.CommentForm.ContainsCommentAsync(body)).Should().BeTrue("comment must appear in the thread");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-6 Issue reopen via UI: returns to Open status")]
    public async Task IssueReopen_ViaUI_ReturnsToOpen()
    {
        var issueNumber = await CreateIssueViaApiAsync(_createdIssueNumbers, isClosed: true);
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.IssueDetailPage.ReopenAsync();

        (await Browser.IssueDetailPage.HasOpenStatusAsync()).Should().BeTrue("status must flip back to Open");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-6 Issue edit title via UI: new title persists")]
    public async Task IssueEdit_ViaUI_TitlePersists()
    {
        var newTitle = $"UI edited {DataGenerator.RandomString(TitleRandomLength)}";
        var issueNumber = await CreateIssueViaApiAsync(_createdIssueNumbers);
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.IssueDetailPage.BeginEditTitleAsync();
        await Browser.IssueDetailPage.SaveTitleAsync(newTitle);

        (await Browser.IssueDetailPage.HasTitleAsync(newTitle)).Should().BeTrue("edited title must persist on reload");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-6 Issue comment add via UI on API-created entity: visible in thread")]
    public async Task IssueComment_Add_ShowsInThread()
    {
        var body = $"Hybrid comment {DataGenerator.RandomString(TitleRandomLength)}";
        var issueNumber = await CreateIssueViaApiAsync(_createdIssueNumbers);
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.CommentForm.AddCommentAsync(body);
        await RegisterCommentIdAsync(issueNumber, body, _createdCommentIds);

        (await Browser.CommentForm.ContainsCommentAsync(body)).Should().BeTrue("added comment must be visible");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var commentId in _createdCommentIds)
        {
            await Cleanup.CleanupCommentAsync(commentId);
        }

        foreach (var issueNumber in _createdIssueNumbers)
        {
            await Cleanup.CleanupIssueAsync(issueNumber);
        }
    }
}
