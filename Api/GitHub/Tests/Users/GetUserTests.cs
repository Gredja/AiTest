using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Users;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetUserTests : GitHubTestBase
{
    private static readonly string _testUsername = TestConfig.GitHubTestUsername;

    [Test]
    [Category("HealthCheck")]
    [Description("2.1 GET /users/{username} returns 200 OK")]
    public async Task GetUser_ReturnsOk()
    {
        var response = await Get<UserModelResponse>(GitHubEndpoints.UsersById, UsernameParam(_testUsername));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("2.2 Response matches expected contract")]
    public async Task GetUser_ResponseMatchesContract()
    {
        var response = await Get<UserModelResponse>(GitHubEndpoints.UsersById, UsernameParam(_testUsername));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("2.3 Response contains valid user fields")]
    public async Task GetUser_HasValidFields()
    {
        var response = await Get<UserModelResponse>(GitHubEndpoints.UsersById, UsernameParam(_testUsername));

        response.Data!.ShouldHaveValidFields();
        response.Data!.Login.Should().Be(_testUsername);
    }

    [Test]
    [Category("Smoke")]
    [Description("2.4 Content-Type is application/json")]
    public async Task GetUser_ContentTypeIsJson()
    {
        var response = await Get<UserModelResponse>(GitHubEndpoints.UsersById, UsernameParam(_testUsername));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Performance")]
    [Description("2.5 Response time < 5 seconds")]
    public async Task GetUser_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<UserModelResponse>(GitHubEndpoints.UsersById, UsernameParam(_testUsername));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Negative")]
    [Description("2.6 Non-existent user returns 404")]
    public async Task GetUser_NonExistentUser_ReturnsNotFound()
    {
        var response = await Get<UserModelResponse>(GitHubEndpoints.UsersById,
            UsernameParam(GitHubEndpoints.NonExistentUser));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("2.7 Login matches request username")]
    public async Task GetUser_LoginMatchesRequest()
    {
        var response = await Get<UserModelResponse>(GitHubEndpoints.UsersById, UsernameParam(_testUsername));

        response.Data!.Login.Should().Be(_testUsername);
    }

    [Test]
    [Category("Regression")]
    [Description("2.8 Repeated calls return same data")]
    public async Task GetUser_RepeatedCalls_ReturnSameData()
    {
        var response1 = await Get<UserModelResponse>(GitHubEndpoints.UsersById, UsernameParam(_testUsername));
        var response2 = await Get<UserModelResponse>(GitHubEndpoints.UsersById, UsernameParam(_testUsername));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Login.Should().Be(response2.Data!.Login);
    }
}
