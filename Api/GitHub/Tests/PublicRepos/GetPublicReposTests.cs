using Core.Models.Generic;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.ParamHelper;

namespace Api.GitHub.PublicRepos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetPublicReposTests : GitHubTestBase
{
    private const int FirstRepositoryId = 1;

    [Test]
    [Category("HealthCheck")]
    [Description("8.1 GET /repositories returns 200 OK")]
    public async Task GetPublicRepos_ReturnsOk()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("8.2 Response matches expected contract")]
    public async Task GetPublicRepos_ResponseMatchesContract()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("8.3 Each public repo has valid fields")]
    public async Task GetPublicRepos_HasValidFields()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories);

        foreach (var repo in response.Data!)
        {
            repo.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("8.4 Pagination with since param limits results")]
    public async Task GetPublicRepos_PaginationWorks()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories,
            [new() { Type = ParamType.Parameter, Key = SinceParamKey, Value = FirstRepositoryId }]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Count.Should().BeGreaterThan(0);
    }

    private const string InvalidQueryValue = "abc";

    [Test]
    [Category("Negative")]
    [Description("8.5 since=abc returns 422")]
    public async Task GetPublicRepos_InvalidSince_Returns422()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories,
            [Query(SinceParamKey, InvalidQueryValue)]);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("8.6 Invalid token returns 401")]
    public async Task GetPublicRepos_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.Repositories, InvalidAuthorization);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }
}
