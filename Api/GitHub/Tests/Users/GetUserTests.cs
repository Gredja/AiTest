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
    private static readonly string _username = GitHubEndpoints.TestUserId;

    [Test]
    [Category("HealthCheck")]
    [Description("2.1 GET /users/{username} returns 200 OK")]
    public async Task GetUser_ReturnsOk()
    {
        var response = await Get<UserModel>(GitHubEndpoints.UsersById, Method.Get, UsernameParam(_username));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("2.2 Response contains valid user fields")]
    public async Task GetUser_HasValidFields()
    {
        var response = await Get<UserModel>(GitHubEndpoints.UsersById, Method.Get, UsernameParam(_username));

        response.Data.Should().NotBeNull();
        response.Data!.Login.Should().Be(_username);
        response.Data.Id.Should().BeGreaterThan(0);
    }

    [Test]
    [Category("Smoke")]
    [Description("2.3 Content-Type is application/json")]
    public async Task GetUser_ContentTypeIsJson()
    {
        var response = await Get<UserModel>(GitHubEndpoints.UsersById, Method.Get, UsernameParam(_username));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Performance")]
    [Description("2.4 Response time < 5 seconds")]
    public async Task GetUser_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<UserModel>(GitHubEndpoints.UsersById, Method.Get, UsernameParam(_username));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(GitHubEndpoints.MaxResponseTimeMs);
    }

    [Test]
    [Category("Negative")]
    [Description("2.5 Non-existent user returns 404")]
    public async Task GetUser_NonExistentUser_ReturnsNotFound()
    {
        var response = await Get<UserModel>(GitHubEndpoints.UsersById, Method.Get,
            UsernameParam(GitHubEndpoints.NonExistentUser));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
