using NUnit.Framework;
using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.GitHub.Helpers.GitHubParamHelper;

namespace Api.GitHub.IssueComments;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetIssueCommentsTests : GitHubTestBase
{
    private const string RepoOwner = "Gredja";
    private const string RepoName = "AiTest";
    private const int ExistingIssueNumber = 5;

    [Test]
    [Category("HealthCheck")]
    [Description("11.1 GET /repos/{owner}/{repo}/issues/{number}/comments returns 200 OK")]
    public async Task GetIssueComments_ReturnsOk()
    {
        var response = await Get<List<CommentModel>>(GitHubEndpoints.RepoIssueComments, Method.Get,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("11.2 Response is an array")]
    public async Task GetIssueComments_ReturnsArray()
    {
        var response = await Get<List<CommentModel>>(GitHubEndpoints.RepoIssueComments, Method.Get,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Smoke")]
    [Description("11.3 Pagination with per_page=2 returns at most 2 comments")]
    public async Task GetIssueComments_PaginationWorks()
    {
        var response = await Get<List<CommentModel>>(GitHubEndpoints.RepoIssueComments, Method.Get,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber), .. PaginationParams(1, 2)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(2);
    }
}
