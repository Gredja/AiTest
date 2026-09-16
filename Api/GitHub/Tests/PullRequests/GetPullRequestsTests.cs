using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.PullRequests;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetPullRequestsTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("5.1 GET /repos/{owner}/{repo}/pulls returns 200 OK")]
    public async Task GetPullRequests_ReturnsOk()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            RepoParam(owner, repo));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("5.2 Each pull request has valid fields")]
    public async Task GetPullRequests_HasValidFields()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            RepoParam(owner, repo));

        foreach (var pr in response.Data!)
        {
            pr.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("5.8 All PR IDs are unique")]
    public async Task GetPullRequests_AllIdsAreUnique()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            RepoParam(owner, repo));

        var ids = response.Data!.Select(pr => pr.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("5.9 Each PR has valid state")]
    public async Task GetPullRequests_EachPRHasValidState()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            RepoParam(owner, repo));

        response.Data.Should().OnlyContain(pr => pr.State == "open" || pr.State == "closed");
    }

    [Test]
    [Category("Negative")]
    [Description("5.10 Non-existent repo returns 404")]
    public async Task GetPullRequests_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            RepoParam(GitHubEndpoints.NonExistentUser, "nonexistent"));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.3 Content-Type is application/json")]
    public async Task GetPullRequests_ContentTypeIsJson()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            RepoParam(owner, repo));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.4 Pagination works with per_page param")]
    public async Task GetPullRequests_PaginationWorks()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            [.. RepoParam(owner, repo), .. PaginationParams(1, 5)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Test]
    [Category("Performance")]
    [Description("5.5 Response time < 5 seconds")]
    public async Task GetPullRequests_ResponseTimeIsAcceptable()
    {
        var (owner, repo) = ParseRepo();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            RepoParam(owner, repo));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.6 Filter by state=open returns only open pull requests")]
    public async Task GetPullRequests_FilterByStateOpen()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            [.. RepoParam(owner, repo), .. StateParam("open")]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        if (response.Data!.Count > 0)
        {
            response.Data.Should().OnlyContain(pr => pr.State == "open");
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("5.7 Filter by state=closed returns only closed pull requests")]
    public async Task GetPullRequests_FilterByStateClosed()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<PullRequestModel>>(GitHubEndpoints.RepoPullRequests, Method.Get,
            [.. RepoParam(owner, repo), .. StateParam("closed")]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        if (response.Data!.Count > 0)
        {
            response.Data.Should().OnlyContain(pr => pr.State == "closed");
        }
    }
}
