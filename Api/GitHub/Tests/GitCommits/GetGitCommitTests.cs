using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
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
    [Category("Negative")]
    [Description("27.2 Non-existent commit sha returns 404")]
    public async Task GetGitCommit_NonExistentSha_ReturnsNotFound()
    {
        var response = await Get<GitCommitModelResponse>(GitHubEndpoints.RepoGitCommitsById,
            [.. TestRepoParam(), .. GitCommitParam(GitHubEndpoints.NonExistentSha)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
