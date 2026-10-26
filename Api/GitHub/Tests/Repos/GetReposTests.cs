using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;

namespace Api.GitHub.Repos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetReposTests : GitHubTestBase
{
    [Test]
    [Category("Negative")]
    [Description("1a.1 GET /repos returns 404 (no such route)")]
    public async Task GetRepos_NoRoute_ReturnsNotFound()
    {
        var response = await Get<object>(GitHubEndpoints.Repos);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("1a.2 404 body carries the documented Not Found message")]
    public async Task GetRepos_NoRoute_BodyHasNotFoundMessage()
    {
        var response = await Get<object>(GitHubEndpoints.Repos);

        var error = response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
        error.Message.Should().Be(GitHubErrors.NotFound);
        error.DocumentationUrl.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Smoke")]
    [Description("1a.3 Content-Type is application/json")]
    public async Task GetRepos_NoRoute_ContentTypeIsJson()
    {
        var response = await Get<object>(GitHubEndpoints.Repos);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("1a.4 Invalid token still returns 404 (route check precedes auth)")]
    public async Task GetRepos_InvalidToken_ReturnsNotFound()
    {
        var response = await ExecuteWithAuthorization(GitHubEndpoints.Repos, InvalidAuthorization);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
