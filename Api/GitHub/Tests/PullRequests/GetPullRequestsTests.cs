using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.PullRequests;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetPullRequestsTests : GitHubTestBase
{
    private static readonly TimeSpan _stateConsistencyTimeout = TimeSpan.FromSeconds(10);

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
    [Description("5.2 Response matches expected contract")]
    public async Task GetPullRequests_ResponseMatchesContract()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);

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

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.ForEach(pullRequest => pullRequest.ShouldHaveValidFields());
    }

    [Test]
    [Category("Regression")]
    [Description("5.4 All PR IDs are unique")]
    public async Task GetPullRequests_AllIdsAreUnique()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        var ids = response.Data!.Select(pullRequest => pullRequest.Id);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("5.5 Each PR has valid state")]
    public async Task GetPullRequests_EachPRHasValidState()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(pullRequest => pullRequest.State == GitHubEndpoints.StateOpen || pullRequest.State == GitHubEndpoints.StateClosed);
    }

    [Test]
    [Category("Negative")]
    [Description("5.6 Non-existent repo returns 404")]
    public async Task GetPullRequests_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.7 Content-Type is application/json")]
    public async Task GetPullRequests_ContentTypeIsJson()
    {
        var response = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
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

        AssertHelper.ShouldBeWithinResponseBudget(stopwatch.ElapsedMilliseconds);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.10 Filter by state=open returns only open pull requests")]
    public async Task GetPullRequests_FilterByStateOpen()
    {
        // GitHub list may serve a torn read while E2E creates/merges PRs concurrently
        // (solution run): a just-merged PR briefly appears in ?state=open — refresh until consistent
        var result = await WaitHelper.WaitUntilAsync(
            () => Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
                [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateOpen)]),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data is not null
                && response.Data.All(pullRequest => pullRequest.State == GitHubEndpoints.StateOpen),
            timeout: _stateConsistencyTimeout);

        result.IsSuccess.Should().BeTrue(
            $"state=open must return only open pull requests; waited {result.Elapsed}, attempts {result.Attempts}, last status {result.LastValue?.StatusCode}");
        result.LastValue!.Data.Should().OnlyContain(pullRequest => pullRequest.State == GitHubEndpoints.StateOpen);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.11 Filter by state=closed returns only closed pull requests")]
    public async Task GetPullRequests_FilterByStateClosed()
    {
        // Symmetric torn read: a just-reopened PR can briefly appear in ?state=closed —
        // refresh until the read is consistent
        var result = await WaitHelper.WaitUntilAsync(
            () => Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
                [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateClosed)]),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data is not null
                && response.Data.All(pullRequest => pullRequest.State == GitHubEndpoints.StateClosed),
            timeout: _stateConsistencyTimeout);

        result.IsSuccess.Should().BeTrue(
            $"state=closed must return only closed pull requests; waited {result.Elapsed}, attempts {result.Attempts}, last status {result.LastValue?.StatusCode}");
        result.LastValue!.Data.Should().OnlyContain(pullRequest => pullRequest.State == GitHubEndpoints.StateClosed);
    }

    [Test]
    [Category("Negative")]
    [Description("5.12 Invalid token returns 401")]
    public async Task GetPullRequests_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoPullRequests, InvalidAuthorization, TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
