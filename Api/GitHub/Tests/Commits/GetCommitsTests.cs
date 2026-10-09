using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Commits;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetCommitsTests : GitHubTestBase
{
    private const int SingleItemPageSize = 1;
    private const string InvalidPerPage = "abc";
    private const string SpecialCharsSegment = "sp@ec!al";
    private const string GitHubUrlPrefix = "https://github.com/";

    [Test]
    [Category("HealthCheck")]
    [Description("15a.1 GET /commits returns 200 OK")]
    public async Task GetCommits_ReturnsOk()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("15a.2 Response matches expected contract")]
    public async Task GetCommits_ResponseMatchesContract()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty("repo must keep data — Entry Criteria, documentation/GitHubTestingStructure.md");
        response.Data.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("15a.3 Fields: sha is 40 hex, commit.message non-empty")]
    public async Task GetCommits_HasValidFields()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(
            commit => commit.Sha.IsGitSha() && commit.Commit.Message.Length > 0,
            "every commit needs a 40-hex sha and a non-empty message");
    }

    [Test]
    [Category("Regression")]
    [Description("15a.4 html_url starts with https://github.com/")]
    public async Task GetCommits_HtmlUrlHasGitHubPrefix()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(
            commit => commit.HtmlUrl.StartsWith(GitHubUrlPrefix, StringComparison.Ordinal),
            "html_url must point at github.com");
    }

    [Test]
    [Category("Smoke")]
    [Description("15a.5 Content-Type is application/json")]
    public async Task GetCommits_ContentTypeIsJson()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            TestRepoParam());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("15a.6 Returns non-empty commit list")]
    public async Task GetCommits_ReturnsNonEmptyList()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty("repo must have at least one commit");
    }

    [Test]
    [Category("Regression")]
    [Description("15a.7 sha values are unique within the list")]
    public async Task GetCommits_ShaValuesAreUnique()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Select(commit => commit.Sha).Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Performance")]
    [Description("15a.8 Response time < 5 seconds")]
    public async Task GetCommits_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            TestRepoParam());
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("15a.9 per_page=1 returns at most 1 commit")]
    public async Task GetCommits_PerPageOne_ReturnsAtMostOne()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            [.. TestRepoParam(), .. PaginationParams(FirstPage, SingleItemPageSize)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(SingleItemPageSize);
    }

    [Test]
    [Category("Smoke")]
    [Description("15a.10 per_page=abc is silently ignored and returns 200")]
    public async Task GetCommits_InvalidPerPage_ReturnsOk()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            [.. TestRepoParam(), .. PerPageParam(InvalidPerPage)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Negative")]
    [Description("15a.11 Non-existent repo returns 404")]
    public async Task GetCommits_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15a.12 Non-existent owner returns 404")]
    public async Task GetCommits_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            RepoParam(GitHubEndpoints.NonExistentUser, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15a.13 Invalid token returns 401")]
    public async Task GetCommits_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoCommits, InvalidAuthorization, TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("15a.14 Owner with special chars returns 404")]
    public async Task GetCommits_OwnerWithSpecialChars_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            RepoParam(SpecialCharsSegment, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15a.15 Repo with special chars returns 404")]
    public async Task GetCommits_RepoWithSpecialChars_ReturnsNotFound()
    {
        var (owner, _) = ParseRepo();

        var response = await Get<List<CommitModelResponse>>(GitHubEndpoints.RepoCommits,
            RepoParam(owner, SpecialCharsSegment));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
