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
public class GetPullRequestByIdTests : GitHubTestBase
{
    private int _existingPullNumber;
    private int _nonExistentPullNumber;

    [OneTimeSetUp]
    public async Task FetchPullNumbers()
    {
        var pulls = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        pulls.ShouldHaveStatusCode(HttpStatusCode.OK);
        pulls.Data.Should().NotBeEmpty("repo must keep at least one PR — Entry Criteria, documentation/GitHubTestingStructure.md");

        _existingPullNumber = pulls.Data!.Max(pullRequest => pullRequest.Number);
        _nonExistentPullNumber = _existingPullNumber + GitHubEndpoints.NonExistentIdOffset;
    }

    [Test]
    [Category("HealthCheck")]
    [Description("9c.1 GET /pulls/{pull_number} for existing PR returns 200 OK")]
    public async Task GetPullRequestById_ReturnsOk()
    {
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("9c.2 Response matches expected contract")]
    public async Task GetPullRequestById_ResponseMatchesContract()
    {
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("9c.3 Fields are valid and number equals requested")]
    public async Task GetPullRequestById_HasValidFields()
    {
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        var pullRequest = response.Data!;
        pullRequest.ShouldHaveValidFields();
        pullRequest.Number.Should().Be(_existingPullNumber, "response must echo the requested PR number");
    }

    [Test]
    [Category("Regression")]
    [Description("9c.4 State is open or closed")]
    public async Task GetPullRequestById_HasValidState()
    {
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.State.Should().BeOneOf(GitHubEndpoints.StateOpen, GitHubEndpoints.StateClosed);
    }

    [Test]
    [Category("Smoke")]
    [Description("9c.5 Content-Type is application/json")]
    public async Task GetPullRequestById_ContentTypeIsJson()
    {
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(_existingPullNumber));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Performance")]
    [Description("9c.6 Response time < 5 seconds")]
    public async Task GetPullRequestById_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(_existingPullNumber));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Negative")]
    [Description("9c.7 PR number 0 returns 404")]
    public async Task GetPullRequestById_ZeroPullNumber_ReturnsNotFound()
    {
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(0));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9c.8 Negative PR number returns 404")]
    public async Task GetPullRequestById_NegativePullNumber_ReturnsNotFound()
    {
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(-1));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9c.9 Non-existent PR returns 404")]
    public async Task GetPullRequestById_NonExistentPull_ReturnsNotFound()
    {
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(_nonExistentPullNumber));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9c.10 Non-existent repo returns 404")]
    public async Task GetPullRequestById_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. PullRequestNumberParam(1)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9c.11 Non-existent owner returns 404")]
    public async Task GetPullRequestById_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, repo), .. PullRequestNumberParam(1)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9c.12 Invalid token returns 401")]
    public async Task GetPullRequestById_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoPullRequestById, InvalidAuthorization, PullParams(1));

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
