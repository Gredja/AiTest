using Api.GitHub.Tests.Support;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Releases;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetReleaseByIdTests : ReleaseTestBase
{
    private const string InvalidIdSegment = "notanumber";

    private string _tagName = string.Empty;
    private long _releaseId;

    [OneTimeSetUp]
    public async Task CreateFixtureRelease() =>
        (_releaseId, _tagName) = await CreateReleaseFixtureAsync();

    [Test]
    [Category("HealthCheck")]
    [Description("15f.1 GET /releases/{release_id} returns 200 OK")]
    public async Task GetReleaseById_ReturnsOk()
    {
        var response = await Get<ReleaseModelResponse>(
            GitHubEndpoints.RepoReleaseById, [.. TestRepoParam(), .. ReleaseIdParam(_releaseId)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("15f.2 Response matches expected contract")]
    public async Task GetReleaseById_ResponseMatchesContract()
    {
        var response = await Get<ReleaseModelResponse>(
            GitHubEndpoints.RepoReleaseById, [.. TestRepoParam(), .. ReleaseIdParam(_releaseId)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("15f.3 Response id and tag_name match the requested release")]
    public async Task GetReleaseById_IdAndTagMatchRequest()
    {
        var response = await Get<ReleaseModelResponse>(
            GitHubEndpoints.RepoReleaseById, [.. TestRepoParam(), .. ReleaseIdParam(_releaseId)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(_releaseId);
        response.Data.TagName.Should().Be(_tagName);
    }

    [Test]
    [Category("Smoke")]
    [Description("15f.4 Public read without auth returns 200 OK")]
    public async Task GetReleaseById_NoAuth_ReturnsOk()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoReleaseById, null, [.. TestRepoParam(), .. ReleaseIdParam(_releaseId)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Negative")]
    [Description("15f.5 Non-existent release id returns 404")]
    public async Task GetReleaseById_NonExistentId_ReturnsNotFound()
    {
        var response = await Get<ReleaseModelResponse>(
            GitHubEndpoints.RepoReleaseById,
            [.. TestRepoParam(), .. ReleaseIdParam(GitHubEndpoints.NonExistentStaticId)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15f.6 Non-numeric release id returns 404")]
    public async Task GetReleaseById_InvalidIdSegment_ReturnsNotFound()
    {
        var response = await Get<ReleaseModelResponse>(
            GitHubEndpoints.RepoReleaseById, [.. TestRepoParam(), .. ReleaseIdParam(InvalidIdSegment)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15f.7 Non-existent repo returns 404")]
    public async Task GetReleaseById_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<ReleaseModelResponse>(
            GitHubEndpoints.RepoReleaseById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. ReleaseIdParam(GitHubEndpoints.NonExistentStaticId)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15f.8 Non-existent owner returns 404")]
    public async Task GetReleaseById_NonExistentOwner_ReturnsNotFound()
    {
        var response = await Get<ReleaseModelResponse>(
            GitHubEndpoints.RepoReleaseById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, ParseRepo().Repo),
             .. ReleaseIdParam(GitHubEndpoints.NonExistentStaticId)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15f.9 Invalid token returns 401")]
    public async Task GetReleaseById_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoReleaseById, InvalidAuthorization,
            [.. TestRepoParam(), .. ReleaseIdParam(_releaseId)]);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await CleanupReleaseFixturesAsync();
}
