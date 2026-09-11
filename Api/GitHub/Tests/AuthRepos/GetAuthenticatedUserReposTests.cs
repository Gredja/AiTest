using NUnit.Framework;
using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.GitHub.Helpers.GitHubParamHelper;

namespace Api.GitHub.AuthRepos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetAuthenticatedUserReposTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Ignore("TODO: investigate 404 — RestSharp URL construction issue with /user/repos")]
    [Description("9.1 GET /user/repos returns 200 OK")]
    public async Task GetAuthenticatedUserRepos_ReturnsOk()
    {
        var response = await Get<List<RepositoryModel>>(GitHubEndpoints.AuthenticatedUserRepos, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Ignore("TODO: investigate 404 — RestSharp URL construction issue with /user/repos")]
    [Description("9.2 Each authenticated user repo has valid fields")]
    public async Task GetAuthenticatedUserRepos_HasValidFields()
    {
        var response = await Get<List<RepositoryModel>>(GitHubEndpoints.AuthenticatedUserRepos, Method.Get);

        response.Data.Should().NotBeNull();
        if (response.Data!.Count > 0)
        {
            var repo = response.Data.First();
            repo.Name.Should().NotBeNullOrWhiteSpace();
            repo.Owner.Should().NotBeNull();
            repo.HtmlUrl.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Test]
    [Category("Smoke")]
    [Ignore("TODO: investigate 404 — RestSharp URL construction issue with /user/repos")]
    [Description("9.3 Pagination with per_page=5 returns at most 5 repos")]
    public async Task GetAuthenticatedUserRepos_PaginationWorks()
    {
        var response = await Get<List<RepositoryModel>>(GitHubEndpoints.AuthenticatedUserRepos, Method.Get,
            PaginationParams(1, 5));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(5);
    }
}
