using System.Text.RegularExpressions;
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
    private static readonly Regex _shaHexPattern = new("^[0-9a-f]+$", RegexOptions.Compiled);

    [Test]
    [Category("HealthCheck")]
    [Description("4.1 GET /repos/{owner}/{repo}/branches returns 200 OK")]
    public async Task GetBranches_ReturnsOk()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("4.2 Response matches expected contract")]
    public async Task GetBranches_ResponseMatchesContract()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("4.3 Each branch has valid fields")]
    public async Task GetBranches_HasValidFields()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            TestRepoParam());

        foreach (var branch in response.Data!)
        {
            branch.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("4.4 Content-Type is application/json")]
    public async Task GetBranches_ContentTypeIsJson()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            TestRepoParam());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("4.5 Pagination works with per_page param")]
    public async Task GetBranches_PaginationWorks()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            [.. TestRepoParam(), .. PaginationParams(FirstPage, SmallPageSize)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(SmallPageSize);
    }

    [Test]
    [Category("Performance")]
    [Description("4.6 Response time < 5 seconds")]
    public async Task GetBranches_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            TestRepoParam());
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("4.7 Pagination with per_page=1 returns at most 1 branch")]
    public async Task GetBranches_PaginationPerOne()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            [.. TestRepoParam(), .. PaginationParams(1, 1)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(1);
    }

    [Test]
    [Category("Regression")]
    [Description("4.8 All branch names are unique")]
    public async Task GetBranches_AllNamesAreUnique()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            TestRepoParam());

        var names = response.Data!.Select(branch => branch.Name);
        names.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("4.9 Each branch commit SHA is 40 hex chars")]
    public async Task GetBranches_EachCommitShaIsValid()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            TestRepoParam());

        response.Data.Should().OnlyContain(branch =>
            branch.Commit.Sha.Length == ShaHexLength &&
            _shaHexPattern.IsMatch(branch.Commit.Sha));
    }

    [Test]
    [Category("Negative")]
    [Description("4.10 Non-existent repo returns 404")]
    public async Task GetBranches_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    private const string NonExistentBranchName = "nonexistent-branch-12345";

    [Test]
    [Category("Negative")]
    [Description("4.11 Non-existent branch returns 404")]
    public async Task GetBranches_NonExistentBranch_ReturnsNotFound()
    {
        var response = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(NonExistentBranchName)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("4.12 Invalid token returns 401")]
    public async Task GetBranches_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoBranches, $"Bearer {InvalidToken}", TestRepoParam());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
