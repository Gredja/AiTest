using System.Text.RegularExpressions;
using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Branches;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetBranchesTests : GitHubTestBase
{
    private const int ShaHexLength = 40;
    private static readonly Regex ShaHexPattern = new("^[0-9a-f]+$", RegexOptions.Compiled);

    [Test]
    [Category("HealthCheck")]
    [Description("4.1 GET /repos/{owner}/{repo}/branches returns 200 OK")]
    public async Task GetBranches_ReturnsOk()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, Method.Get,
            RepoParam(owner, repo));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("4.2 Each branch has valid fields")]
    public async Task GetBranches_HasValidFields()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, Method.Get,
            RepoParam(owner, repo));

        foreach (var branch in response.Data!)
        {
            branch.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("4.3 Content-Type is application/json")]
    public async Task GetBranches_ContentTypeIsJson()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, Method.Get,
            RepoParam(owner, repo));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("4.4 Pagination works with per_page param")]
    public async Task GetBranches_PaginationWorks()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, Method.Get,
            [.. RepoParam(owner, repo), .. PaginationParams(1, 2)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Test]
    [Category("Performance")]
    [Description("4.5 Response time < 5 seconds")]
    public async Task GetBranches_ResponseTimeIsAcceptable()
    {
        var (owner, repo) = ParseRepo();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, Method.Get,
            RepoParam(owner, repo));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("4.6 Pagination with per_page=1 returns at most 1 branch")]
    public async Task GetBranches_PaginationPerOne()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, Method.Get,
            [.. RepoParam(owner, repo), .. PaginationParams(1, 1)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(1);
    }

    [Test]
    [Category("Regression")]
    [Description("4.7 All branch names are unique")]
    public async Task GetBranches_AllNamesAreUnique()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, Method.Get,
            RepoParam(owner, repo));

        var names = response.Data!.Select(b => b.Name).ToList();
        names.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("4.8 Each branch commit SHA is 40 hex chars")]
    public async Task GetBranches_EachCommitShaIsValid()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, Method.Get,
            RepoParam(owner, repo));

        response.Data.Should().OnlyContain(b =>
            b.Commit.Sha.Length == ShaHexLength &&
            ShaHexPattern.IsMatch(b.Commit.Sha));
    }

    [Test]
    [Category("Negative")]
    [Description("4.9 Non-existent repo returns 404")]
    public async Task GetBranches_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, Method.Get,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
