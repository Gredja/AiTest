using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.GitRefs;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetGitRefByNameTests : GitHubTestBase
{
    private static string DefaultBranchRef => $"{GitHubEndpoints.HeadsPrefix}{GitHubEndpoints.DefaultBranch}";

    [Test]
    [Category("HealthCheck")]
    [Description("26a.1 GET /git/ref/{ref} returns 200 OK")]
    public async Task GetGitRefByName_ReturnsOk()
    {
        var response = await Get<GitRefModelResponse>(GitHubEndpoints.RepoGitRefByName,
            [.. TestRepoParam(), .. GitRefParam(DefaultBranchRef)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("26a.2 Response matches expected contract")]
    public async Task GetGitRefByName_ResponseMatchesContract()
    {
        var response = await Get<GitRefModelResponse>(GitHubEndpoints.RepoGitRefByName,
            [.. TestRepoParam(), .. GitRefParam(DefaultBranchRef)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Negative")]
    [Description("26a.3 Non-existent ref returns 404")]
    public async Task GetGitRefByName_NonExistentRef_ReturnsNotFound()
    {
        var response = await Get<GitRefModelResponse>(GitHubEndpoints.RepoGitRefByName,
            [.. TestRepoParam(), .. GitRefParam($"{GitHubEndpoints.HeadsPrefix}{GitHubEndpoints.NonExistentBranchName}")]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("26a.4 Non-existent repo returns 404")]
    public async Task GetGitRefByName_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<GitRefModelResponse>(GitHubEndpoints.RepoGitRefByName,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. GitRefParam(DefaultBranchRef)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
