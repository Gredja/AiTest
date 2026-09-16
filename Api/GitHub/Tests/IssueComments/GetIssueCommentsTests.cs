using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.IssueComments;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetIssueCommentsTests : GitHubTestBase
{
    private const string RepoOwner = "Gredja";
    private const string RepoName = "AiTest";
    private const int ExistingIssueNumber = 5;
    private const int NonExistentIssueNumber = 99999;

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

    [Test]
    [Category("Regression")]
    [Description("11.4 Each comment has valid fields")]
    public async Task GetIssueComments_HasValidFields()
    {
        var response = await Get<List<CommentModel>>(GitHubEndpoints.RepoIssueComments, Method.Get,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        foreach (var comment in response.Data!)
        {
            comment.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("11.5 All comment IDs are unique")]
    public async Task GetIssueComments_AllIdsAreUnique()
    {
        var response = await Get<List<CommentModel>>(GitHubEndpoints.RepoIssueComments, Method.Get,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(ExistingIssueNumber)]);

        var ids = response.Data!.Select(c => c.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Negative")]
    [Description("11.6 Non-existent issue returns 404")]
    public async Task GetIssueComments_NonExistentIssue_ReturnsNotFound()
    {
        var response = await Get<List<CommentModel>>(GitHubEndpoints.RepoIssueComments, Method.Get,
            [.. RepoParam(RepoOwner, RepoName), .. IssueNumberParam(NonExistentIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("11.7 Non-existent repo returns 404")]
    public async Task GetIssueComments_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<CommentModel>>(GitHubEndpoints.RepoIssueComments, Method.Get,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName), .. IssueNumberParam(1)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
