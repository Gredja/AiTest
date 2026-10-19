using AllureAdapter;
using Core.Helpers;
using FluentAssertions;
using Ui.Github.Tests.Support;
using static Core.Helpers.TestDataConstants;

namespace Ui.Github.Tests.GitHubStream.Repository;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class BranchesUiTests : GitHubUiTestBase
{
    private readonly List<string> _createdBranches = [];

    [Test]
    [Category("Regression")]
    [Description("UI-13 Branch create via UI: appears in branches list")]
    public async Task BranchCreate_ViaUI_AppearsInBranches()
    {
        var branchName = $"ui-ui-{DataGenerator.RandomString(RandomNameLength)}";
        _createdBranches.Add(branchName);

        await Browser.BranchesPage.OpenAsync();
        await Browser.BranchesPage.CreateBranchAsync(branchName);

        var found = await Browser.BranchesPage.WaitForBranchAsync(branchName);
        found.IsSuccess.Should().BeTrue(
            $"branch must appear in the branches list (attempts={found.Attempts}, elapsed={found.Elapsed})");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-13 Branch list: API-created branch visible in sandbox")]
    public async Task BranchList_Sandbox_ShowsApiBranch()
    {
        var branchName = $"ui-api-{DataGenerator.RandomString(RandomNameLength)}";
        await CreateBranchViaApiAsync(branchName, _createdBranches);

        await Browser.BranchesPage.OpenAsync();
        var found = await Browser.BranchesPage.WaitForBranchAsync(branchName);

        found.IsSuccess.Should().BeTrue(
            $"API-created branch must be visible (attempts={found.Attempts}, elapsed={found.Elapsed})");
    }

    [Test]
    [Category("Regression")]
    [Description("UI-13 Branch delete via UI: branch is gone from list")]
    public async Task BranchDelete_ViaUI_IsGone()
    {
        var branchName = $"ui-del-{DataGenerator.RandomString(RandomNameLength)}";
        await CreateBranchViaApiAsync(branchName, _createdBranches);

        await Browser.BranchesPage.OpenAsync();
        await Browser.BranchesPage.DeleteBranchAsync(branchName);

        (await Browser.BranchesPage.HasBranchAsync(branchName)).Should().BeFalse("deleted branch must be gone");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-14 Branch compare: commits shown between base and head")]
    public async Task BranchCompare_Default_ShowsCommits()
    {
        var (headBranch, baseBranch) = await CreateDivergentBranchesViaApiAsync(_createdBranches);

        await Browser.PullCreatePage.OpenCompareAsync(baseBranch, headBranch);
        var commitsShown = await Browser.PullCreatePage.HasCommitsSectionAsync();

        commitsShown.Should().BeTrue("compare view must list commits between divergent branches");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var branchName in _createdBranches)
        {
            await Cleanup.CleanupGitRefAsync(branchName);
        }
    }
}
