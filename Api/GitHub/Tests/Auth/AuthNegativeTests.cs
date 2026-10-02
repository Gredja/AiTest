using Core.Config;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using RestSharp;
using TestAdapter;

namespace Api.GitHub.Auth;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class AuthNegativeTests : GitHubTestBase
{
    private const string InvalidToken = "ghp_invalidtoken123";
    private const string MalformedAuthorization = "Basic !!!not-a-token!!!";

    [Test]
    [Category("Negative")]
    [Description("12.1 GET /user/repos with invalid token returns 401")]
    public async Task GetUserRepos_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            CreateRequest(GitHubEndpoints.AuthenticatedUserRepos), $"Bearer {InvalidToken}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("12.2 GET /user/repos without Authorization header returns 401")]
    public async Task GetUserRepos_MissingAuthorization_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            CreateRequest(GitHubEndpoints.AuthenticatedUserRepos), authorization: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("12.3 GET /user/repos with malformed Authorization header returns 401")]
    public async Task GetUserRepos_MalformedAuthorization_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            CreateRequest(GitHubEndpoints.AuthenticatedUserRepos), MalformedAuthorization);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("12.4 GET /repos/{owner}/{repo} with invalid token returns 401")]
    public async Task GetRepository_InvalidToken_ReturnsUnauthorized()
    {
        var (owner, repo) = ParseRepo();
        var request = CreateRequest(GitHubEndpoints.ReposById);
        request.AddUrlSegment("owner", owner);
        request.AddUrlSegment("repo", repo);

        var response = await ExecuteWithAuthorization(request, $"Bearer {InvalidToken}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("12.5 GET /rate_limit with invalid token returns 401")]
    public async Task GetRateLimit_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            CreateRequest(GitHubEndpoints.RateLimit), $"Bearer {InvalidToken}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static RestRequest CreateRequest(string endpoint) =>
        new(endpoint) { RequestFormat = DataFormat.Json };

    private async Task<RestResponse> ExecuteWithAuthorization(RestRequest request, string? authorization)
    {
        if (authorization is not null)
        {
            request.AddHeader("Authorization", authorization);
        }

        return await Client.ExecuteAsync(request);
    }
}
