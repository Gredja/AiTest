using Core.Models.GitHub;
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
public class GetPullRequestFilesTests : GitHubTestBase
{
    private static readonly Regex _shaHexPattern = new("^[0-9a-f]{40}$", RegexOptions.Compiled);

    private static readonly string[] ValidFileStatuses = ["added", "modified", "removed", "renamed", "changed"];

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
    [Description("9a.1 GET /pulls/{pull_number}/files for existing PR returns 200 OK")]
    public async Task GetPullRequestFiles_ReturnsOk()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("9a.2 Response matches expected contract")]
    public async Task GetPullRequestFiles_ResponseMatchesContract()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty("PR should have changed files for contract check");
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("9a.3 Each file has valid sha, filename and status")]
    public async Task GetPullRequestFiles_HasValidFields()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(_existingPullNumber));

        response.Data!.ForEach(file =>
        {
            file.Sha.Should().MatchRegex(_shaHexPattern, "sha must be 40 hex chars");
            file.Filename.Should().NotBeNullOrWhiteSpace();
            file.Status.Should().BeOneOf(ValidFileStatuses);
        });
    }

    [Test]
    [Category("Smoke")]
    [Description("9a.4 Content-Type is application/json")]
    public async Task GetPullRequestFiles_ContentTypeIsJson()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(_existingPullNumber));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("9a.5 Response contains non-empty file list")]
    public async Task GetPullRequestFiles_ReturnsNonEmptyList()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(_existingPullNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty("PR has changed files");
    }

    [Test]
    [Category("Regression")]
    [Description("9a.6 Filename values are unique within PR")]
    public async Task GetPullRequestFiles_FilenamesAreUnique()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(_existingPullNumber));

        response.Data!.Select(file => file.Filename).Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("9a.7 additions/deletions/changes are >= 0")]
    public async Task GetPullRequestFiles_ChangeCountsAreNonNegative()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(_existingPullNumber));

        response.Data!.ForEach(file =>
        {
            file.Additions.Should().BeGreaterThanOrEqualTo(0);
            file.Deletions.Should().BeGreaterThanOrEqualTo(0);
            file.Changes.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Test]
    [Category("Performance")]
    [Description("9a.8 Response time < 5 seconds")]
    public async Task GetPullRequestFiles_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(_existingPullNumber));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Negative")]
    [Description("9a.9 Non-existent PR returns 404")]
    public async Task GetPullRequestFiles_NonExistentPullRequest_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(_nonExistentPullNumber));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9a.10 PR number 0 returns 404")]
    public async Task GetPullRequestFiles_ZeroPullNumber_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(0));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9a.11 Negative PR number returns 404")]
    public async Task GetPullRequestFiles_NegativePullNumber_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            PullParams(-1));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9a.12 Non-existent repo returns 404")]
    public async Task GetPullRequestFiles_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. PullRequestNumberParam(_existingPullNumber)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9a.13 Non-existent owner returns 404")]
    public async Task GetPullRequestFiles_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<PullRequestFileModelResponse>>(GitHubEndpoints.RepoPullRequestFiles,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, repo),
             .. PullRequestNumberParam(_existingPullNumber)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("9a.14 Invalid token returns 401")]
    public async Task GetPullRequestFiles_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoPullRequestFiles, InvalidAuthorization, PullParams(_existingPullNumber));

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
