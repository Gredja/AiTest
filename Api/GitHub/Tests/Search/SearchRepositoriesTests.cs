using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Search;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class SearchRepositoriesTests : GitHubTestBase
{
    private const string RepositoryQuery = "user:Gredja";

    [Test]
    [Category("HealthCheck")]
    [Description("29.1 GET /search/repositories returns 200 OK")]
    public async Task SearchRepositories_ReturnsOk()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories, SearchQueryParam(RepositoryQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("29.2 Response matches expected contract")]
    public async Task SearchRepositories_ResponseMatchesContract()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories, SearchQueryParam(RepositoryQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Negative")]
    [Description("29.3 Missing q returns 422")]
    public async Task SearchRepositories_InvalidQuery_Returns422()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }
}
