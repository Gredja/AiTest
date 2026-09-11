using NUnit.Framework;
using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.GitHub.Helpers.GitHubParamHelper;

namespace Api.GitHub.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetIssueByIdTests : GitHubTestBase
{
    private const string RepoOwner = "Gredja";
    private const string RepoName = "AiTest";
    private const int ExistingIssueNumber = 5;

    [Test]
    [Category("HealthCheck")]
    [Description("10.1 GET /repos/{owner}/{repo}/issues/{number} returns 200 OK")]
    public async Task GetIssueById_ReturnsOk()
    {
        var response = await Get<IssueModel>(GitHubEndpoints.RepoIssueById, Method.Get,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("10.2 Response has valid issue fields")]
    public async Task GetIssueById_HasValidFields()
    {
        var response = await Get<IssueModel>(GitHubEndpoints.RepoIssueById, Method.Get,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        response.Data.Should().NotBeNull();
        response.Data!.Title.Should().NotBeNullOrWhiteSpace();
        response.Data.State.Should().NotBeNullOrWhiteSpace();
        response.Data.User.Should().NotBeNull();
    }

    [Test]
    [Category("Negative")]
    [Description("10.3 Non-existent issue returns 404")]
    public async Task GetIssueById_NonExistentIssue_ReturnsNotFound()
    {
        var response = await Get<IssueModel>(GitHubEndpoints.RepoIssueById, Method.Get,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(99999)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
