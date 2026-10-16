using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;

namespace Api.GitHub.Auth;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetAuthenticatedUserTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("5a.1 GET /user returns 200 OK")]
    public async Task GetAuthenticatedUser_ReturnsOk()
    {
        var response = await Get<UserModelResponse>(GitHubEndpoints.AuthenticatedUser);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("5a.2 Response matches expected contract")]
    public async Task GetAuthenticatedUser_ResponseMatchesContract()
    {
        var response = await Get<UserModelResponse>(GitHubEndpoints.AuthenticatedUser);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("5a.3 Authenticated login matches the test account")]
    public async Task GetAuthenticatedUser_HasValidFields()
    {
        var response = await Get<UserModelResponse>(GitHubEndpoints.AuthenticatedUser);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.ShouldHaveValidFields();
        response.Data!.Login.Should().Be(TestConfig.GitHubTestUsername);
    }

    [Test]
    [Category("Negative")]
    [Description("5a.4 No auth returns 401")]
    public async Task GetAuthenticatedUser_NoAuth_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(GitHubEndpoints.AuthenticatedUser, null);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("5a.5 Invalid token returns 401")]
    public async Task GetAuthenticatedUser_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(GitHubEndpoints.AuthenticatedUser, InvalidAuthorization);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
