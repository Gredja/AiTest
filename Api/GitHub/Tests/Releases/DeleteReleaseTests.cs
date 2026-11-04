using Api.GitHub.Helpers;
using Core.Config;
using Core.Helpers;
using System.Net;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Releases;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class DeleteReleaseTests : ReleaseTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("15e.1 DELETE /releases/{release_id} returns 204")]
    public async Task DeleteRelease_ReturnsNoContent()
    {
        var (releaseId, _) = await CreateReleaseFixtureAsync();

        var deleted = await Delete<object>(
            GitHubEndpoints.RepoReleaseById, [.. TestRepoParam(), .. ReleaseIdParam(releaseId)]);

        deleted.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Test]
    [Category("Regression")]
    [Description("15e.2 Second delete of the same release returns 404")]
    public async Task DeleteRelease_Twice_ReturnsNotFound()
    {
        var (releaseId, _) = await CreateReleaseFixtureAsync();

        var first = await Delete<object>(
            GitHubEndpoints.RepoReleaseById, [.. TestRepoParam(), .. ReleaseIdParam(releaseId)]);
        first.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        var second = await Delete<object>(
            GitHubEndpoints.RepoReleaseById, [.. TestRepoParam(), .. ReleaseIdParam(releaseId)]);
        second.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15e.3 Non-existent release id returns 404")]
    public async Task DeleteRelease_NonExistentId_ReturnsNotFound()
    {
        var response = await Delete<object>(
            GitHubEndpoints.RepoReleaseById,
            [.. TestRepoParam(), .. ReleaseIdParam(GitHubEndpoints.NonExistentStaticId)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15e.4 Non-existent repo returns 404")]
    public async Task DeleteRelease_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Delete<object>(
            GitHubEndpoints.RepoReleaseById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. ReleaseIdParam(GitHubEndpoints.NonExistentStaticId)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15e.5 Non-existent owner returns 404")]
    public async Task DeleteRelease_NonExistentOwner_ReturnsNotFound()
    {
        var response = await Delete<object>(
            GitHubEndpoints.RepoReleaseById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, ParseRepo().Repo),
             .. ReleaseIdParam(GitHubEndpoints.NonExistentStaticId)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15e.6 DELETE without auth returns 401")]
    public async Task DeleteRelease_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoReleaseById, Method.Delete);
        AddParams(request, [.. TestRepoParam(), .. ReleaseIdParam(GitHubEndpoints.NonExistentStaticId)]);

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("15e.7 Invalid token returns 401")]
    public async Task DeleteRelease_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoReleaseById, Method.Delete);
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        AddParams(request, [.. TestRepoParam(), .. ReleaseIdParam(GitHubEndpoints.NonExistentStaticId)]);

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await CleanupReleaseFixturesAsync();
}
