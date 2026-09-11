using NUnit.Framework;
using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.GitHub.Helpers.GitHubParamHelper;

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

        response.Data.Should().NotBeNull();
        var pr = response.Data!.FirstOrDefault();
        if (pr != null)
        {
            pr.Id.Should().BeGreaterThan(0);
            pr.Title.Should().NotBeNullOrWhiteSpace();
            pr.State.Should().NotBeNullOrWhiteSpace();
            pr.User.Should().NotBeNull();
        }
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

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(GitHubEndpoints.MaxResponseTimeMs);
    }
}
