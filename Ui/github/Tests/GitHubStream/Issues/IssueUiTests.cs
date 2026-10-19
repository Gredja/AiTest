using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using Core.Models.GitHub;
using FluentAssertions;
using AllureAdapter;
using Ui.Github.Tests.Support;
using static Core.Helpers.TestDataConstants;

namespace Ui.Github.Tests.GitHubStream.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class IssueUiTests : GitHubUiTestBase
{
    private const string BugLabel = "bug";

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

        (await Browser.CommentForm.HasCommentAsync(body)).Should().BeTrue("comment must appear in the thread");
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

        (await Browser.CommentForm.HasCommentAsync(body)).Should().BeTrue("added comment must be visible");
    }

    [Test]
    [Category("Negative")]
    [Description("UI-20 Issue create form: empty title submit shows validation error")]
    public async Task IssueCreate_EmptyTitle_ViaUI_ShowsValidationError()
    {
        await Browser.IssueCreatePage.OpenAsync();
        var opened = await Browser.IssueCreatePage.WaitForOpenAsync();
        opened.IsSuccess.Should().BeTrue(
            $"create form must open (attempts={opened.Attempts}, elapsed={opened.Elapsed})");

        var validation = await Browser.IssueCreatePage.SubmitEmptyTitleAsync();

        validation.IsSuccess.Should().BeTrue(
            $"validation message must appear (attempts={validation.Attempts}, elapsed={validation.Elapsed})");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-20 Issue create form: Cancel returns to the issues list")]
    public async Task IssueCreate_Cancel_ReturnsToList()
    {
        // Cancel goes back in the session history — seed the list page first so "back"
        // lands on it instead of the blank initial page of the fresh context
        await Browser.IssuesPage.OpenAsync();
        await Browser.IssueCreatePage.OpenAsync();
        var opened = await Browser.IssueCreatePage.WaitForOpenAsync();
        opened.IsSuccess.Should().BeTrue(
            $"create form must open (attempts={opened.Attempts}, elapsed={opened.Elapsed})");

        var cancelled = await Browser.IssueCreatePage.CancelAsync();

        cancelled.IsSuccess.Should().BeTrue(
            $"cancel must return to the issues list (attempts={cancelled.Attempts}, elapsed={cancelled.Elapsed})");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-18 Issue labels picker: label added via UI shows on the issue detail, then removes cleanly")]
    public async Task IssueLabel_Add_ShowsOnIssue()
    {
        var issueNumber = await CreateIssueViaApiAsync(_createdIssueNumbers);
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        // the picker commits the label when it closes — poll the API only afterwards
        await Browser.IssueDetailPage.OpenLabelsPickerAsync();
        var selected = await Browser.IssueDetailPage.SelectLabelAsync(BugLabel);
        selected.IsSuccess.Should().BeTrue(
            $"label option must toggle to selected (attempts={selected.Attempts}, elapsed={selected.Elapsed})");
        await Browser.IssueDetailPage.CloseLabelsPickerAsync();

        var added = await WaitForIssueLabelAsync(issueNumber, BugLabel, hasLabel: true);
        added.IsSuccess.Should().BeTrue(
            $"label must be readable via API (attempts={added.Attempts}, elapsed={added.Elapsed})");
        (await Browser.IssueDetailPage.HasLabelChipAsync(BugLabel)).Should().BeTrue(
            "label chip must show on the issue detail");

        await Browser.IssueDetailPage.OpenLabelsPickerAsync();
        var deselected = await Browser.IssueDetailPage.DeselectLabelAsync(BugLabel);
        deselected.IsSuccess.Should().BeTrue(
            $"label option must toggle back (attempts={deselected.Attempts}, elapsed={deselected.Elapsed})");
        await Browser.IssueDetailPage.CloseLabelsPickerAsync();

        var removed = await WaitForIssueLabelAsync(issueNumber, BugLabel, hasLabel: false);
        removed.IsSuccess.Should().BeTrue(
            $"label must be removable again (attempts={removed.Attempts}, elapsed={removed.Elapsed})");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-19 Comment actions: edited body persists in the thread")]
    public async Task CommentEdit_ViaUI_TextPersists()
    {
        var originalBody = $"UI comment {DataGenerator.RandomString(TitleRandomLength)}";
        var editedBody = $"UI edited comment {DataGenerator.RandomString(TitleRandomLength)}";
        var issueNumber = await CreateIssueViaApiAsync(_createdIssueNumbers);
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.CommentForm.AddCommentAsync(originalBody);
        await RegisterCommentIdAsync(issueNumber, originalBody, _createdCommentIds);

        await Browser.CommentForm.EditFirstCommentAsync(editedBody);

        (await Browser.CommentForm.HasCommentAsync(editedBody)).Should().BeTrue(
            "edited body must render in the thread");
        (await Browser.CommentForm.HasCommentAsync(originalBody)).Should().BeFalse(
            "old body must be gone after the update");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-19 Comment actions: deleted comment leaves the thread and the API")]
    public async Task CommentDelete_ViaUI_IsGone()
    {
        var body = $"UI delete comment {DataGenerator.RandomString(TitleRandomLength)}";
        var issueNumber = await CreateIssueViaApiAsync(_createdIssueNumbers);
        await Browser.IssueDetailPage.OpenAsync(issueNumber);

        await Browser.CommentForm.AddCommentAsync(body);
        await RegisterCommentIdAsync(issueNumber, body, _createdCommentIds);

        var deleted = await Browser.CommentForm.DeleteFirstCommentAsync(body);
        deleted.IsSuccess.Should().BeTrue(
            $"comment must leave the thread (attempts={deleted.Attempts}, elapsed={deleted.Elapsed})");

        var gone = await WaitHelper.WaitUntilAsync(
            async () =>
            {
                var response = await Cleanup.Get<List<CommentModelResponse>>(
                    GitHubEndpoints.RepoIssueComments,
                    [.. UiSandboxRepoParams(), .. GitHubParamHelper.IssueNumberParam(issueNumber)]);

                return !(response.Data?.Any(comment => comment.Body == body) ?? false);
            },
            isGone => isGone);
        gone.IsSuccess.Should().BeTrue(
            $"comment must be gone via API (attempts={gone.Attempts}, elapsed={gone.Elapsed})");
    }

    [Test]
    [Category("Negative")]
    [Description("UI-23 Non-existent issue: 404 illustration shown")]
    public async Task IssueNotFound_ViaUI_Shows404()
    {
        var nonExistentNumber = await ResolveNonExistentIssueNumberAsync();
        await Browser.IssueDetailPage.OpenAsync(nonExistentNumber);

        var notFound = await Browser.NotFoundPage.WaitFor404MessageAsync();
        notFound.IsSuccess.Should().BeTrue(
            $"GitHub serves the 404 illustration for missing issues (attempts={notFound.Attempts}, elapsed={notFound.Elapsed})");
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
