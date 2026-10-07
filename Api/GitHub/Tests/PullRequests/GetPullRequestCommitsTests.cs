using Core.Models.GitHub;
using Core.Models.Generic;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.PullRequests;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetPullRequestCommitsTests : GitHubTestBase
{
    private const string Sha40HexPattern = "^[0-9a-f]{40}$";
    private const string GithubUrlPrefix = "https://github.com/";

    private int _existingPullNumber;
    private int _nonExistentPullNumber;

    [OneTimeSetUp]
    public async Task FetchPullNumbers()
    {
        var pulls = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);

        _existingPullNumber = pulls.Data!.Max(pullRequest => pullRequest.Number);
        _nonExistentPullNumber = _existingPullNumber + GitHubEndpoints.NonExistentIdOffset;
    }

    private List<RequestDictionaryModel> PullCommitsParams(int pullNumber) =>
        [.. TestRepoParam(), .. PullRequestNumberParam(pullNumber)];

    [Test]
    [Category("HealthCheck")]
    [Description("9b.1 GET /pulls/{pull_number}/commits for existing PR returns 200 OK")]
    public async Task GetPullRequestCommits_ReturnsOk()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("9b.2 Response matches expected contract")]
    public async Task GetPullRequestCommits_ResponseMatchesContract()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty("PR should have commits for contract check");
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("9b.3 Each commit has valid unique sha")]
    public async Task GetPullRequestCommits_ShasAreValidAndUnique()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(_existingPullNumber));

        foreach (var pullRequestCommit in response.Data!)
        {
            pullRequestCommit.Sha.Should().MatchRegex(Sha40HexPattern, "sha must be 40 hex chars");
        }

        response.Data.Select(pullRequestCommit => pullRequestCommit.Sha).Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("9b.4 commit.message is non-empty")]
    public async Task GetPullRequestCommits_MessageIsNotEmpty()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(_existingPullNumber));

        foreach (var pullRequestCommit in response.Data!)
        {
            pullRequestCommit.Commit.Message.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("9b.5 Content-Type is application/json")]
    public async Task GetPullRequestCommits_ContentTypeIsJson()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(_existingPullNumber));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("9b.6 Response contains non-empty commit list")]
    public async Task GetPullRequestCommits_ReturnsNonEmptyList()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty("PR has commits");
    }

    [Test]
    [Category("Regression")]
    [Description("9b.7 html_url starts with https://github.com/")]
    public async Task GetPullRequestCommits_HtmlUrlHasGithubPrefix()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(_existingPullNumber));

        foreach (var pullRequestCommit in response.Data!)
        {
            pullRequestCommit.HtmlUrl.Should().StartWith(GithubUrlPrefix);
        }
    }

    [Test]
    [Category("Performance")]
    [Description("9b.8 Response time < 5 seconds")]
    public async Task GetPullRequestCommits_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(_existingPullNumber));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Negative")]
    [Description("9b.9 Non-existent PR returns 404")]
    public async Task GetPullRequestCommits_NonExistentPullRequest_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(_nonExistentPullNumber));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9b.10 PR number 0 returns 404")]
    public async Task GetPullRequestCommits_ZeroPullNumber_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(0));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9b.11 Negative PR number returns 404")]
    public async Task GetPullRequestCommits_NegativePullNumber_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            PullCommitsParams(-1));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9b.12 Non-existent repo returns 404")]
    public async Task GetPullRequestCommits_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. PullRequestNumberParam(_existingPullNumber)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9b.13 Non-existent owner returns 404")]
    public async Task GetPullRequestCommits_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<PullRequestCommitModelResponse>>(GitHubEndpoints.RepoPullRequestCommits,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, repo),
             .. PullRequestNumberParam(_existingPullNumber)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9b.14 Invalid token returns 401")]
    public async Task GetPullRequestCommits_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoPullRequestCommits, InvalidAuthorization, PullCommitsParams(_existingPullNumber));

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
