using Core.Models.GitHub;
using JsonFields = Core.Models.GitHub.JsonFields;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using static Core.Helpers.ParamHelper;
using static Core.Helpers.TestDataConstants;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.PublicRepos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetPublicReposTests : GitHubTestBase
{
    private const int FirstRepositoryId = 1;
    private const long HugeSinceId = 999999999;
    private const int MaxPerPageCap = 100;

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

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.ForEach(repo => repo.ShouldHaveValidFields());
    }

    [Test]
    [Category("Smoke")]
    [Description("8.4 Pagination with since param limits results")]
    public async Task GetPublicRepos_PaginationWorks()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories,
            [Query(JsonFields.Since, FirstRepositoryId)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Count.Should().BeGreaterThan(0);
    }

    [Test]
    [Category("Negative")]
    [Description("8.5 since=abc returns 422")]
    public async Task GetPublicRepos_InvalidSince_Returns422()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories,
            [Query(JsonFields.Since, NonNumericValue)]);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("8.6 Invalid token returns 401")]
    public async Task GetPublicRepos_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.Repositories, InvalidAuthorization);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Performance")]
    [Description("8.7 Response time < 5 seconds")]
    public async Task GetPublicRepos_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories);
        stopwatch.Stop();

        AssertHelper.ShouldBeWithinResponseBudget(stopwatch.ElapsedMilliseconds);
    }

    [Test]
    [Category("Negative")]
    [Description("8.8 since=999999999 returns only repos with greater ids (200)")]
    public async Task GetPublicRepos_SinceBeyondAllIds_ReturnsFilteredRepos()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories,
            [Query(JsonFields.Since, HugeSinceId)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty("public repos with id >= 1e9 exist (probe 2026-11-02)");
        response.Data.Should().OnlyContain(repository => repository.Id > HugeSinceId);
    }

    [Test]
    [Category("Negative")]
    [Description("8.9 per_page=0 falls back to the default page (200)")]
    public async Task GetPublicRepos_PerPageZero_ReturnsDefaultPage()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories,
            PerPageParam(DefaultingPerPage));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty("per_page=0 must fall back to the default page, not an empty one");
    }

    [Test]
    [Category("Negative")]
    [Description("8.10 per_page=101 is capped at 100 items (200)")]
    public async Task GetPublicRepos_PerPageOverMax_ReturnsCappedPage()
    {
        var response = await Get<List<RepositoryModelResponse>>(GitHubEndpoints.Repositories,
            PerPageParam(MaxPerPageCap + 1));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().Be(MaxPerPageCap);
    }
}
