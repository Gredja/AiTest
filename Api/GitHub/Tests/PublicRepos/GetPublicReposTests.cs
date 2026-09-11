using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.GitHub.Helpers.GitHubParamHelper;

namespace Api.GitHub.PublicRepos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetPublicReposTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("8.1 GET /repositories returns 200 OK")]
    public async Task GetPublicRepos_ReturnsOk()
    {
        var response = await Get<List<RepositoryModel>>(GitHubEndpoints.Repositories, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("8.2 Each public repo has valid fields")]
    public async Task GetPublicRepos_HasValidFields()
    {
        var response = await Get<List<RepositoryModel>>(GitHubEndpoints.Repositories, Method.Get);

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
    [Description("8.3 Pagination with since param limits results")]
    public async Task GetPublicRepos_PaginationWorks()
    {
        var response = await Get<List<RepositoryModel>>(GitHubEndpoints.Repositories, Method.Get,
            new() { new() { Type = ParamType.Parameter, Key = "since", Value = 1 } });

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Count.Should().BeGreaterThan(0);
    }
}
