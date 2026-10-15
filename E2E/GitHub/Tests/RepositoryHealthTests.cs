using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using AllureAdapter;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class RepositoryHealthTests : GitHubE2ETestBase
{
    [Test]
    [Category("Regression")]
    [Description("E2E-4 Repository health: default branch, topics, visibility")]
    public async Task RepositoryHealth_VerifyDefaultBranch_Topics_Visibility()
    {
        var repo = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, TestRepoParam());
        repo.ShouldHaveStatusCode(HttpStatusCode.OK);
        repo.Data!.DefaultBranch.Should().Be(GitHubEndpoints.DefaultBranch,
            "sandbox must keep main as default branch — Entry Criteria, documentation/GitHubTestingStructure.md");
        repo.Data.IsPrivate.Should().BeFalse("sandbox repo must stay public");

        var branches = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, TestRepoParam());
        branches.ShouldHaveStatusCode(HttpStatusCode.OK);
        var defaultBranch = branches.Data!.FirstOrDefault(branch => branch.Name == GitHubEndpoints.DefaultBranch);
        defaultBranch.Should().NotBeNull("default branch must exist among repo branches");

        var topics = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics, MercyPreviewRepoParams());
        topics.ShouldHaveStatusCode(HttpStatusCode.OK);
        topics.Data!.Names.Should().OnlyContain(
            name => TopicNamePattern.IsMatch(name),
            "topic names must be lowercase alphanumeric with hyphens");
    }
}
