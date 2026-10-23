using AllureAdapter;
using Core.Helpers;
using FluentAssertions;
using Ui.Github.Tests.Support;
using static Core.Helpers.TestDataConstants;

namespace Ui.Github.Tests.GitHubStream.Pulls;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class PullFlowUiTests : GitHubUiTestBase
{
    private readonly List<(int Number, string HeadBranch, string BaseBranch)> _createdPullRequests = [];
    private readonly List<string> _createdBranches = [];
    private readonly List<long> _createdCommentIds = [];

    [Test]
    [Category("Regression")]
    [Description("UI-11 Pull request create via UI: appears in list")]
    public async Task PullCreate_ViaUI_AppearsInList()
    {
        var title = $"UI create {DataGenerator.RandomString(TitleRandomLength)}";
        var (headBranch, baseBranch) = await CreateDivergentBranchesViaApiAsync(_createdBranches);

        await Browser.PullCreatePage.OpenCompareAsync(baseBranch, headBranch);
        await Browser.PullCreatePage.CreatePullRequestAsync(title, "Created through the UI compare form");

        var pullNumber = Browser.PullCreatePage.TryGetPullNumberFromUrl();
        if (pullNumber is not null)
        {
            _createdPullRequests.Add((pullNumber.Value, headBranch, baseBranch));
        }
        else
        {
            await RegisterPullByTitleAsync(title, headBranch, baseBranch, _createdPullRequests);
        }

        pullNumber.Should().NotBeNull("submit must redirect to the pull request page");

        await Browser.PullsPage.OpenAsync();
        var found = await Browser.PullsPage.WaitForPullWithTitleAsync(title);
        found.IsSuccess.Should().BeTrue(
            $"pull request must appear in the list (attempts={found.Attempts}, elapsed={found.Elapsed})");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-10 Pull request detail: title and base/head branches shown")]
    public async Task PullDetail_Open_ShowsTitleAndBranches()
    {
        var title = $"UI detail {DataGenerator.RandomString(TitleRandomLength)}";
        var (pullNumber, headBranch, baseBranch) =
            await CreateScratchPullRequestViaApiAsync(title, _createdPullRequests, _createdBranches);

        await Browser.PullDetailPage.OpenAsync(pullNumber);

        (await Browser.PullDetailPage.HasTitleAsync(title)).Should().BeTrue("pull request page must show the title");
        (await Browser.PullDetailPage.HasBranchLinkAsync(baseBranch)).Should().BeTrue("base branch must be linked");
        (await Browser.PullDetailPage.HasBranchLinkAsync(headBranch)).Should().BeTrue("head branch must be linked");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-10 Pull request edit title via UI: new title persists")]
    public async Task PullEdit_ViaUI_TitlePersists()
    {
        var newTitle = $"UI edited {DataGenerator.RandomString(TitleRandomLength)}";
        var (pullNumber, _, _) =
            await CreateScratchPullRequestViaApiAsync($"UI title {DataGenerator.RandomString(TitleRandomLength)}", _createdPullRequests, _createdBranches);

        await Browser.PullDetailPage.OpenAsync(pullNumber);
        await Browser.PullDetailPage.BeginEditTitleAsync();
        await Browser.PullDetailPage.SaveTitleAsync(newTitle);

        (await Browser.PullDetailPage.HasTitleAsync(newTitle)).Should().BeTrue("edited title must persist on the page");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-10 Pull request edit body via UI: new body persists")]
    public async Task PullEdit_ViaUI_BodyPersists()
    {
        var newBody = $"UI body {DataGenerator.RandomString(BodyRandomLength)}";
        var (pullNumber, _, _) =
            await CreateScratchPullRequestViaApiAsync($"UI body {DataGenerator.RandomString(TitleRandomLength)}", _createdPullRequests, _createdBranches);

        await Browser.PullDetailPage.OpenAsync(pullNumber);
        await Browser.PullDetailPage.EditBodyAsync(newBody);

        (await Browser.PullDetailPage.HasTextAsync(newBody)).Should().BeTrue("edited body must persist on the page");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-10 Pull comment create via UI: visible in thread")]
    public async Task PullComment_Create_ShowsInThread()
    {
        var body = $"UI comment {DataGenerator.RandomString(TitleRandomLength)}";
        var (pullNumber, _, _) =
            await CreateScratchPullRequestViaApiAsync($"UI comment setup {DataGenerator.RandomString(TitleRandomLength)}", _createdPullRequests, _createdBranches);

        await Browser.PullDetailPage.OpenAsync(pullNumber);
        await Browser.PullDetailPage.AddCommentAsync(body);
        await RegisterCommentIdAsync(pullNumber, body, _createdCommentIds);

        (await Browser.PullDetailPage.HasTextAsync(body)).Should().BeTrue("comment must appear in the thread");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-10 Pull request close via UI: shows Closed status")]
    public async Task PullClose_ViaUI_ShowsClosedBadge()
    {
        var (pullNumber, _, _) =
            await CreateScratchPullRequestViaApiAsync($"UI close {DataGenerator.RandomString(TitleRandomLength)}", _createdPullRequests, _createdBranches);

        await Browser.PullDetailPage.OpenAsync(pullNumber);
        await Browser.PullDetailPage.CloseAsync();

        (await Browser.PullDetailPage.HasClosedStatusAsync()).Should().BeTrue("status must flip to Closed");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-10 Pull request reopen via UI: returns to Open status")]
    public async Task PullReopen_ViaUI_ReturnsToOpen()
    {
        var (pullNumber, _, _) =
            await CreateScratchPullRequestViaApiAsync($"UI reopen {DataGenerator.RandomString(TitleRandomLength)}", _createdPullRequests, _createdBranches);
        await ClosePullRequestViaApiAsync(pullNumber);

        await Browser.PullDetailPage.OpenAsync(pullNumber);
        await Browser.PullDetailPage.ReopenAsync();

        (await Browser.PullDetailPage.HasOpenStatusAsync()).Should().BeTrue("status must flip back to Open");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-10 Pull request convert to draft via UI: shows Draft status")]
    public async Task PullDraft_MarkAsDraft_ShowsBadge()
    {
        var (pullNumber, _, _) =
            await CreateScratchPullRequestViaApiAsync($"UI draft {DataGenerator.RandomString(TitleRandomLength)}", _createdPullRequests, _createdBranches);

        await Browser.PullDetailPage.OpenAsync(pullNumber);
        var found = await Browser.PullDetailPage.ConvertToDraftAsync();

        found.IsSuccess.Should().BeTrue(
            $"status must show Draft (attempts={found.Attempts}, elapsed={found.Elapsed})");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-12 Pull request files changed: scratch file from API commit visible")]
    public async Task PullFiles_Change_ShowsChangedFile()
    {
        var (pullNumber, _, _) =
            await CreateScratchPullRequestViaApiAsync($"UI files {DataGenerator.RandomString(TitleRandomLength)}", _createdPullRequests, _createdBranches);

        await Browser.PullDetailPage.OpenFilesChangedAsync(pullNumber);
        var found = await Browser.PullDetailPage.WaitForFileAsync(ScratchFileName);

        found.IsSuccess.Should().BeTrue(
            $"changed file must be visible in the diff (attempts={found.Attempts}, elapsed={found.Elapsed})");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var commentId in _createdCommentIds)
        {
            await Cleanup.CleanupCommentAsync(commentId);
        }

        foreach (var (pullNumber, headBranch, baseBranch) in _createdPullRequests)
        {
            await Cleanup.CleanupPullRequestAsync(pullNumber, headBranch, baseBranch);
        }

        foreach (var branchName in _createdBranches)
        {
            await Cleanup.CleanupGitRefAsync(branchName);
        }
    }
}
