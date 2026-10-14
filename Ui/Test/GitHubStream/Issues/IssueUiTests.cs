using Core.Config;
using Core.Helpers;
using Core.Models.GitHub;
using FluentAssertions;
using AllureAdapter;
using System.Net;
using Ui.Helper;

namespace Ui.Test.GitHubStream.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class IssueUiTests : GitHubUiTestBase
{
    private const int TitleRandomLength = 12;

    private readonly List<int> _createdIssueNumbers = [];
    private readonly List<long> _createdCommentIds = [];

    [Test]
    [Category("Regression")]
    [Description("UI-1 Issue create via UI: appears in list, registered for cleanup")]
    public async Task IssueCreate_ViaUI_AppearsInList()
    {
        var title = $"UI create {DataGenerator.RandomString(TitleRandomLength)}";

        await Browser.IssueCreatePage.OpenAsync();
        (await Browser.IssueCreatePage.IsOpenAsync()).Should().BeTrue("create form must open");
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
    [Description("UI-2 Issue close via UI: shows Closed status")]
    public async Task IssueClose_ViaUI_ShowsClosedBadge()
    {
        var issueNumber = await CreateIssueViaApiAsync();
        var title = $"UI close {DataGenerator.RandomString(TitleRandomLength)}";
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.IssueDetailPage.CloseAsync();

        (await Browser.IssueDetailPage.HasClosedStatusAsync()).Should().BeTrue("status must flip to Closed");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-3 Comment create via UI: visible in thread")]
    public async Task CommentCreate_ViaUI_IsVisible()
    {
        var body = $"UI comment {DataGenerator.RandomString(TitleRandomLength)}";
        var issueNumber = await CreateIssueViaApiAsync();
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.CommentForm.AddCommentAsync(body);

        (await Browser.CommentForm.ContainsCommentAsync(body)).Should().BeTrue("comment must appear in the thread");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-5 Issue reopen via UI: returns to Open status")]
    public async Task IssueReopen_ViaUI_ReturnsToOpen()
    {
        var issueNumber = await CreateIssueViaApiAsync(closed: true);
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
        var issueNumber = await CreateIssueViaApiAsync();
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.IssueDetailPage.BeginEditTitleAsync();
        await Browser.IssueDetailPage.SaveTitleAsync(newTitle);

        (await Browser.IssueDetailPage.HasTitleAsync(newTitle)).Should().BeTrue("edited title must persist on reload");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-7 Issue comment add via UI on API-created entity: visible in thread")]
    public async Task IssueComment_Add_ShowsInThread()
    {
        var body = $"Hybrid comment {DataGenerator.RandomString(TitleRandomLength)}";
        var issueNumber = await CreateIssueViaApiAsync();
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.CommentForm.AddCommentAsync(body);

        (await Browser.CommentForm.ContainsCommentAsync(body)).Should().BeTrue("added comment must be visible");
    }

    private async Task<int> CreateIssueViaApiAsync(bool closed = false)
    {
        var title = $"API setup {DataGenerator.RandomString(TitleRandomLength)}";
        var created = await Cleanup.Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues,
            new CreateIssueModelRequest { Title = title },
            RepoParams());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        var issueNumber = created.Data!.Number;
        _createdIssueNumbers.Add(issueNumber);

        if (closed)
        {
            await Cleanup.CleanupIssueAsync(issueNumber);
        }

        return issueNumber;
    }

    private static List<Core.Models.Generic.RequestDictionaryModel> RepoParams()
    {
        var parts = TestConfig.UiTestRepo.Split('/');

        return Core.Helpers.GitHub.GitHubParamHelper.RepoParam(parts[0], parts[1]);
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
