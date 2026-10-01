using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetIssueByIdTests : GitHubTestBase
{
    private const int ExistingIssueNumber = 5;

    [Test]
    [Category("HealthCheck")]
    [Description("10.1 GET /repos/{owner}/{repo}/issues/{number} returns 200 OK")]
    public async Task GetIssueById_ReturnsOk()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("10.2 Response matches expected contract")]
    public async Task GetIssueById_ResponseMatchesContract()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("10.3 Response has valid issue fields")]
    public async Task GetIssueById_HasValidFields()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Negative")]
    [Description("10.4 Non-existent issue returns 404")]
    public async Task GetIssueById_NonExistentIssue_ReturnsNotFound()
    {
        var (owner, repo) = ParseRepo();
        var issues = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. RepoParam(owner, repo), .. StateParam(GitHubEndpoints.StateAll)]);
        var maxIssueNumber = issues.Data!.Max(issue => issue.Number);
        var nonExistentIssueNumber = maxIssueNumber + 1;

        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(owner, repo), .. IssueNumberParam(nonExistentIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("10.5 Issue number 0 returns 404")]
    public async Task GetIssueById_ZeroIssueNumber_ReturnsNotFound()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(owner, repo), .. IssueNumberParam(0)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("10.6 Negative issue number returns 404")]
    public async Task GetIssueById_NegativeIssueNumber_ReturnsNotFound()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(owner, repo), .. IssueNumberParam(-1)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("10.7 Title matches expected issue")]
    public async Task GetIssueById_TitleIsNotEmpty()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        response.Data!.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Regression")]
    [Description("10.8 Repeated calls return same data")]
    public async Task GetIssueById_RepeatedCalls_ReturnSameData()
    {
        var (owner, repo) = ParseRepo();
        var response1 = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);
        var response2 = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Title.Should().Be(response2.Data!.Title);
    }
}
