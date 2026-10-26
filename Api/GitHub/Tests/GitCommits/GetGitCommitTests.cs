using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.GitCommits;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetGitCommitTests : GitHubTestBase
{
    private string _mainCommitSha = string.Empty;

    [OneTimeSetUp]
    public async Task ResolveMainCommitSha() => _mainCommitSha = await ResolveDefaultBranchShaAsync();

    [Test]
    [Category("HealthCheck")]
    [Description("27.1 GET /git/commits/{commit_sha} returns 200 OK")]
    public async Task GetGitCommit_ReturnsOk()
    {
        var response = await Get<GitCommitModelResponse>(GitHubEndpoints.RepoGitCommitsById,
            [.. TestRepoParam(), .. GitCommitParam(_mainCommitSha)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("27.1a Response matches expected contract")]
    public async Task GetGitCommit_ResponseMatchesContract()
    {
        var response = await Get<GitCommitModelResponse>(GitHubEndpoints.RepoGitCommitsById,
            [.. TestRepoParam(), .. GitCommitParam(_mainCommitSha)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("27.1b Response sha matches the requested commit")]
    public async Task GetGitCommit_ShaMatchesRequest()
    {
        var response = await Get<GitCommitModelResponse>(GitHubEndpoints.RepoGitCommitsById,
            [.. TestRepoParam(), .. GitCommitParam(_mainCommitSha)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Sha.Should().Be(_mainCommitSha);
        response.Data.Sha.ShouldHaveValidSha();
        response.Data.Tree.Should().NotBeNull();
        response.Data.Tree.Sha.ShouldHaveValidSha();
    }

    [Test]
    [Category("Negative")]
    [Description("27.2 Non-existent commit sha returns 404")]
    public async Task GetGitCommit_NonExistentSha_ReturnsNotFound()
    {
        var response = await Get<GitCommitModelResponse>(GitHubEndpoints.RepoGitCommitsById,
            [.. TestRepoParam(), .. GitCommitParam(GitHubEndpoints.NonExistentSha)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("27.2a Non-existent repo returns 404")]
    public async Task GetGitCommit_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<GitCommitModelResponse>(GitHubEndpoints.RepoGitCommitsById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. GitCommitParam(_mainCommitSha)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Smoke")]
    [Description("27.1c Content-Type is application/json")]
    public async Task GetGitCommit_ContentTypeIsJson()
    {
        var response = await Get<GitCommitModelResponse>(GitHubEndpoints.RepoGitCommitsById,
            [.. TestRepoParam(), .. GitCommitParam(_mainCommitSha)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }
}
