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
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Ignore("Creating PR requires branch + commit — too complex for Setup")]
    [Description("5.2 Response matches expected contract")]
    public async Task GetPullRequests_ResponseMatchesContract()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty("Repo should have PRs for contract check");
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("5.3 Each pull request has valid fields")]
    public async Task GetPullRequests_HasValidFields()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());

        foreach (var pullRequest in response.Data!)
        {
            pullRequest.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("5.4 All PR IDs are unique")]
    public async Task GetPullRequests_AllIdsAreUnique()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());

        var ids = response.Data!.Select(pullRequest => pullRequest.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("5.5 Each PR has valid state")]
    public async Task GetPullRequests_EachPRHasValidState()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());

        response.Data.Should().OnlyContain(pullRequest => pullRequest.State == GitHubEndpoints.StateOpen || pullRequest.State == GitHubEndpoints.StateClosed);
    }

    [Test]
    [Category("Negative")]
    [Description("5.6 Non-existent repo returns 404")]
    public async Task GetPullRequests_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.7 Content-Type is application/json")]
    public async Task GetPullRequests_ContentTypeIsJson()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.8 Pagination works with per_page param")]
    public async Task GetPullRequests_PaginationWorks()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. PaginationParams(FirstPage, DefaultPageSize)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(DefaultPageSize);
    }

    [Test]
    [Category("Performance")]
    [Description("5.9 Response time < 5 seconds")]
    public async Task GetPullRequests_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.10 Filter by state=open returns only open pull requests")]
    public async Task GetPullRequests_FilterByStateOpen()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateOpen)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(pullRequest => pullRequest.State == GitHubEndpoints.StateOpen);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.11 Filter by state=closed returns only closed pull requests")]
    public async Task GetPullRequests_FilterByStateClosed()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateClosed)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(pullRequest => pullRequest.State == GitHubEndpoints.StateClosed);
    }
}
