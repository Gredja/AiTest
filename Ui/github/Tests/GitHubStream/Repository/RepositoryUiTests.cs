using FluentAssertions;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using Core.Models.GitHub;
using System.Net;
using Ui.Github.Tests.Support;
using static Core.Helpers.TestDataConstants;

namespace Ui.Github.Tests.GitHubStream.Repository;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class RepositoryUiTests : GitHubUiTestBase
{
    private const string UiSandboxFolder = "ui-scratch-dir";

    private readonly List<string> _createdBranches = [];

    [Test]
    [Category("Smoke")]
    [Description("UI-3 Repository landing: default branch main, README visible")]
    public async Task RepositoryLanding_DefaultBranchAndReadme_Verify()
    {
        await Browser.RepositoryPage.OpenAsync();

        var branch = await Browser.RepositoryPage.GetDefaultBranchAsync();
        branch.Should().Contain(GitHubEndpoints.DefaultBranch, "sandbox repo default branch is main");

        var hasReadme = await Browser.RepositoryPage.HasReadmeAsync();
        hasReadme.Should().BeTrue("repo root must list README.md");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-8 Code view: file tree contains README.md")]
    public async Task CodeView_ReadmeVisible_Verify()
    {
        await Browser.CodePage.OpenAsync();

        var hasReadme = await Browser.CodePage.HasReadmeAsync();
        hasReadme.Should().BeTrue("tree/main must list README.md");
    }

    [Test]
    [Category("Negative")]
    [Description("UI-9 Non-existent repo: 404 illustration message shown")]
    public async Task NotFoundPage_Shows404Message()
    {
        await Browser.NotFoundPage.OpenAsync();

        var notFound = await Browser.NotFoundPage.WaitFor404MessageAsync();
        notFound.IsSuccess.Should().BeTrue(
            $"GitHub serves the 404 illustration for missing repos (attempts={notFound.Attempts}, elapsed={notFound.Elapsed})");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-21 Repository landing About: API description renders on the page")]
    public async Task RepoAbout_ViaUI_ShowsDescription()
    {
        var repository = await Cleanup.Get<RepositoryModelResponse>(
            GitHubEndpoints.ReposById,
            UiSandboxRepoParams());
        repository.ShouldHaveStatusCode(HttpStatusCode.OK);
        repository.Data!.Description.Should().NotBeNullOrWhiteSpace("sandbox repo must have a description to assert");

        await Browser.RepositoryPage.OpenAsync();
        var occurrences = await Browser.RepositoryPage.GetDescriptionOccurrencesAsync(repository.Data.Description);

        occurrences.Should().BeGreaterThanOrEqualTo(2,
            "the API description must render both in About and in the README section");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-21 Code view: folder on a scratch branch lists its files")]
    public async Task CodeView_Folder_ShowsFiles()
    {
        var branchName = $"{UiSandboxFolder}-{DataGenerator.RandomString(RandomNameLength)}";
        await CreateFolderBranchViaApiAsync(branchName, UiSandboxFolder, _createdBranches);

        await Browser.CodePage.OpenFolderAsync(branchName, UiSandboxFolder);
        var hasFile = await Browser.CodePage.HasFileAsync(ScratchFileName);

        hasFile.Should().BeTrue("the folder must list the file created via the git API");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-21 Code file: README.md renders its markdown content")]
    public async Task CodeFile_Readme_RendersContent()
    {
        await Browser.CodePage.OpenReadmeFileAsync();

        var hasContent = await Browser.CodePage.HasRenderedReadmeContentAsync();
        hasContent.Should().BeTrue("README.md must render its own heading as markdown");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-21 Commits: latest main commit message shown on the commits page")]
    public async Task RepoCommits_ViaUI_ShowsLatestCommit()
    {
        var commits = await Cleanup.Get<List<CommitModelResponse>>(
            GitHubEndpoints.RepoCommits,
            [.. UiSandboxRepoParams(), .. GitHubParamHelper.PerPageParam(1)]);
        commits.ShouldHaveStatusCode(HttpStatusCode.OK);
        commits.Data.Should().NotBeEmpty("repo must have at least one commit — Entry Criteria");
        var latestMessage = commits.Data![0].Commit.Message.Split('\n')[0].Trim();

        await Browser.CodePage.OpenCommitsAsync();
        var found = await Browser.CodePage.WaitForCommitAsync(latestMessage);

        found.IsSuccess.Should().BeTrue(
            $"latest commit must appear on the commits page (attempts={found.Attempts}, elapsed={found.Elapsed})");
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
