using NUnit.Framework;
using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.GitHub.Helpers.GitHubParamHelper;

namespace Api.GitHub.Users;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetUserTests : GitHubTestBase
{
    private static readonly string Username = GitHubEndpoints.TestUserId;

    [Test]
    [Category("HealthCheck")]
    [Description("2.1 GET /users/{username} returns 200 OK")]
    public async Task GetUser_ReturnsOk()
    {
        var response = await Get<UserModel>(GitHubEndpoints.UsersById, Method.Get, UsernameParam(Username));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("2.2 Response contains valid user fields")]
    public async Task GetUser_HasValidFields()
    {
        var response = await Get<UserModel>(GitHubEndpoints.UsersById, Method.Get, UsernameParam(Username));

        response.Data.Should().NotBeNull();
        response.Data!.Login.Should().Be(Username);
        response.Data.Id.Should().BeGreaterThan(0);
    }

    [Test]
    [Category("Smoke")]
    [Description("2.3 Content-Type is application/json")]
    public async Task GetUser_ContentTypeIsJson()
    {
        var response = await Get<UserModel>(GitHubEndpoints.UsersById, Method.Get, UsernameParam(Username));

        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Category("Performance")]
    [Description("2.4 Response time < 5 seconds")]
    public async Task GetUser_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<UserModel>(GitHubEndpoints.UsersById, Method.Get, UsernameParam(Username));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(GitHubEndpoints.MaxResponseTimeMs);
    }
}
