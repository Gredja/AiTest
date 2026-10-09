using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Branches;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetBranchByNameTests : GitHubTestBase
{

    private const int MaxBranchesPage = 100;
    private string _slashBranchName = string.Empty;

    [OneTimeSetUp]
    public async Task ResolveSlashBranchName()
    {
        var branches = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches,
            [.. TestRepoParam(), .. PerPageParam(MaxBranchesPage)]);

        branches.ShouldHaveStatusCode(HttpStatusCode.OK);
        _slashBranchName = branches.Data!
            .Select(branch => branch.Name)
            .FirstOrDefault(name => name.Contains('/')) ?? string.Empty;
        _slashBranchName.Should().NotBeNullOrWhiteSpace(
            "test repo must keep at least one branch with a slash — Entry Criteria, documentation/GitHubTestingStructure.md");
    }

    [Test]
    [Category("HealthCheck")]
    [Description("11.1 GET /branches/{branch} for default branch returns 200 OK")]
    public async Task GetBranchByName_ReturnsOk()
    {
        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("11.2 Response matches expected contract")]
    public async Task GetBranchByName_ResponseMatchesContract()
    {
        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("11.3 name equals requested branch")]
    public async Task GetBranchByName_NameMatchesRequestedBranch()
    {
        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);

        response.Data!.Name.Should().Be(GitHubEndpoints.DefaultBranch);
    }

    [Test]
    [Category("Regression")]
    [Description("11.4 commit.sha is 40 hex chars")]
    public async Task GetBranchByName_CommitShaIsValid()
    {
        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);

        response.Data!.Commit.Sha.ShouldHaveValidSha();
    }

    [Test]
    [Category("Smoke")]
    [Description("11.5 Content-Type is application/json")]
    public async Task GetBranchByName_ContentTypeIsJson()
    {
        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("11.6 Branch name with slash returns 200 and name matches")]
    public async Task GetBranchByName_BranchWithSlash_ReturnsOk()
    {
        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(_slashBranchName)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Name.Should().Be(_slashBranchName);
    }

    [Test]
    [Category("Negative")]
    [Description("11.7 Non-existent branch returns 404")]
    public async Task GetBranchByName_NonExistentBranch_ReturnsNotFound()
    {
        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.NonExistentBranchName)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.BranchNotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("11.8 Branch name with spaces returns 404")]
    public async Task GetBranchByName_BranchWithSpaces_ReturnsNotFound()
    {
        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.BranchWithSpacesName)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.BranchNotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("11.9 Non-existent repo returns 404")]
    public async Task GetBranchByName_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("11.10 Non-existent owner returns 404")]
    public async Task GetBranchByName_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, repo),
             .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("11.11 Invalid token returns 401")]
    public async Task GetBranchByName_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoBranchByName, InvalidAuthorization,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
