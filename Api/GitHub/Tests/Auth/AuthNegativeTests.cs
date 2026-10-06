using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.GitHub.Auth;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class AuthNegativeTests : GitHubTestBase
{
    private const string MalformedAuthorization = "Basic !!!not-a-token!!!";

    [Test]
    [Category("Negative")]
    [Description("12.1 GET /user/repos with invalid token returns 401")]
    public async Task GetUserRepos_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.AuthenticatedUserRepos, InvalidAuthorization);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("12.2 GET /user/repos without Authorization header returns 401")]
    public async Task GetUserRepos_MissingAuthorization_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.AuthenticatedUserRepos, authorization: null);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("12.3 GET /user/repos with malformed Authorization header returns 401")]
    public async Task GetUserRepos_MalformedAuthorization_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.AuthenticatedUserRepos, MalformedAuthorization);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("12.4 GET /repos/{owner}/{repo} with invalid token returns 401")]
    public async Task GetRepository_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.ReposById, InvalidAuthorization, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("12.5 GET /rate_limit with invalid token returns 401")]
    public async Task GetRateLimit_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RateLimit, InvalidAuthorization);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }
}
