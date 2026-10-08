using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Users;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetUsersTests : GitHubTestBase
{
    private const int SingleItemPageSize = 1;
    private const string InvalidPerPage = "abc";

    [Test]
    [Category("HealthCheck")]
    [Description("3a.1 GET /users returns 200 OK")]
    public async Task GetUsers_ReturnsOk()
    {
        var response = await Get<List<UserModelResponse>>(GitHubEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("3a.2 Response matches expected contract")]
    public async Task GetUsers_ResponseMatchesContract()
    {
        var response = await Get<List<UserModelResponse>>(GitHubEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("3a.3 Each user has valid fields")]
    public async Task GetUsers_HasValidFields()
    {
        var response = await Get<List<UserModelResponse>>(GitHubEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        foreach (var user in response.Data!)
        {
            user.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("3a.4 Content-Type is application/json")]
    public async Task GetUsers_ContentTypeIsJson()
    {
        var response = await Get<List<UserModelResponse>>(GitHubEndpoints.Users);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("3a.5 Returns non-empty user list")]
    public async Task GetUsers_ReturnsNonEmptyList()
    {
        var response = await Get<List<UserModelResponse>>(GitHubEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty("users list endpoint must return at least one user");
    }

    [Test]
    [Category("Regression")]
    [Description("3a.6 Login values are unique")]
    public async Task GetUsers_LoginsAreUnique()
    {
        var response = await Get<List<UserModelResponse>>(GitHubEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Select(user => user.Login).Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Performance")]
    [Description("3a.7 Response time < 5 seconds")]
    public async Task GetUsers_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<UserModelResponse>>(GitHubEndpoints.Users);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("3a.8 per_page=1 returns exactly 1 user")]
    public async Task GetUsers_PerPageOne_ReturnsSingleUser()
    {
        var response = await Get<List<UserModelResponse>>(GitHubEndpoints.Users,
            [.. PaginationParams(FirstPage, SingleItemPageSize)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().Be(SingleItemPageSize);
    }

    [Test]
    [Category("Smoke")]
    [Description("3a.9 per_page=abc is silently ignored and returns 200")]
    public async Task GetUsers_InvalidPerPage_ReturnsOk()
    {
        var response = await Get<List<UserModelResponse>>(GitHubEndpoints.Users,
            [.. PerPageParam(InvalidPerPage)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Negative")]
    [Description("3a.10 Invalid token returns 401")]
    public async Task GetUsers_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.Users, InvalidAuthorization);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
