using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.UserRepos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetUserReposTests : GitHubTestBase
{
    private const string TestUserId = "Gredja";
    [Test]
    [Category("HealthCheck")]
    [Description("7.1 GET /users/{username}/repos returns 200 OK")]
    public async Task GetUserRepos_ReturnsOk()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.UsersRepos,
            UsernameParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("7.2 Response matches expected contract")]
    public async Task GetUserRepos_ResponseMatchesContract()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.UsersRepos,
            UsernameParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("7.3 Each user repo has valid fields")]
    public async Task GetUserRepos_HasValidFields()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.UsersRepos,
            UsernameParam(TestUserId));

        foreach (var repo in response.Data!)
        {
            repo.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Negative")]
    [Description("7.4 Non-existent user repos returns 404")]
    public async Task GetUserRepos_NonExistentUser_ReturnsNotFound()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.UsersRepos,
            UsernameParam(GitHubEndpoints.NonExistentUser));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Smoke")]
    [Description("7.5 Pagination with per_page=5 returns at most 5 repos")]
    public async Task GetUserRepos_PaginationWorks()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.UsersRepos,
            [.. UsernameParam(TestUserId), .. PaginationParams(1, 5)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(5);
    }
}
