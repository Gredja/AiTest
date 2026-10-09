using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.AuthRepos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetAuthenticatedUserReposTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("9.1 GET /user/repos returns 200 OK")]
    public async Task GetAuthenticatedUserRepos_ReturnsOk()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.AuthenticatedUserRepos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("9.2 Response matches expected contract")]
    public async Task GetAuthenticatedUserRepos_ResponseMatchesContract()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.AuthenticatedUserRepos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty("user must have repos — Entry Criteria, documentation/GitHubTestingStructure.md");
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("9.3 Each authenticated user repo has valid fields")]
    public async Task GetAuthenticatedUserRepos_HasValidFields()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.AuthenticatedUserRepos);

        response.Data.Should().NotBeNull();
        response.Data!.ForEach(repo => repo.ShouldHaveValidFields());
    }

    [Test]
    [Category("Smoke")]
    [Description("9.4 Pagination with per_page=5 returns at most 5 repos")]
    public async Task GetAuthenticatedUserRepos_PaginationWorks()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.AuthenticatedUserRepos,
            PaginationParams(FirstPage, DefaultPageSize));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(DefaultPageSize);
    }

    [Test]
    [Category("Negative")]
    [Description("9.5 Invalid token returns 401")]
    public async Task GetAuthenticatedUserRepos_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.AuthenticatedUserRepos, InvalidAuthorization);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Performance")]
    [Description("9.6 Response time < 5 seconds")]
    public async Task GetAuthenticatedUserRepos_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.AuthenticatedUserRepos);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
