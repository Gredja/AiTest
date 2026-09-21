using RestSharp;
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
    private const string RepoOwner = "Gredja";
    private const string RepoName = "AiTest";
    private const int ExistingIssueNumber = 5;
    private const int NonExistentIssueNumber = 99999;

    [Test]
    [Category("HealthCheck")]
    [Description("10.1 GET /repos/{owner}/{repo}/issues/{number} returns 200 OK")]
    public async Task GetIssueById_ReturnsOk()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("10.2 Response matches expected contract")]
    public async Task GetIssueById_ResponseMatchesContract()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("10.3 Response has valid issue fields")]
    public async Task GetIssueById_HasValidFields()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Negative")]
    [Description("10.4 Non-existent issue returns 404")]
    public async Task GetIssueById_NonExistentIssue_ReturnsNotFound()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(NonExistentIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("10.5 Issue number 0 returns 404")]
    public async Task GetIssueById_ZeroIssueNumber_ReturnsNotFound()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(0)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("10.6 Negative issue number returns 404")]
    public async Task GetIssueById_NegativeIssueNumber_ReturnsNotFound()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(-1)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("10.7 Title matches expected issue")]
    public async Task GetIssueById_TitleIsNotEmpty()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        response.Data!.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Regression")]
    [Description("10.8 Repeated calls return same data")]
    public async Task GetIssueById_RepeatedCalls_ReturnSameData()
    {
        var response1 = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);
        var response2 = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Title.Should().Be(response2.Data!.Title);
    }
}
