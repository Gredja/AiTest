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
    private const int ExistingIssueNumber = 5;
    private const string TestComment = "Test comment for contract check";

    private long? _createdCommentId;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        if (response.Data!.Count == 0)
        {
            var create = await Post<CreateCommentModelRequest, CommentModelResponse>(
                GitHubEndpoints.RepoIssueComments,
                new CreateCommentModelRequest { Body = TestComment },
                [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);
            _createdCommentId = create.Data!.Id;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdCommentId.HasValue)
        {
            var (owner, repo) = ParseRepo();
            try
            {
                var delete = await Delete<object>(
                    GitHubEndpoints.RepoIssueCommentById,
                    [.. RepoParam(owner, repo), .. CommentIdParam(_createdCommentId.Value)]);

                if (!delete.IsSuccessful)
                {
                    TestContext.Progress.WriteLine(
                        $"Warning: comment {_createdCommentId} not deleted: HTTP {(int)delete.StatusCode}");
                }
            }
            catch (HttpRequestException exception)
            {
                TestContext.Progress.WriteLine(
                    $"Warning: failed to delete comment {_createdCommentId}: {exception.Message}");
            }
        }
    }

    [Test]
    [Category("HealthCheck")]
    [Description("11.1 GET /repos/{owner}/{repo}/issues/{number}/comments returns 200 OK")]
    public async Task GetIssueComments_ReturnsOk()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("11.2 Response matches expected contract")]
    public async Task GetIssueComments_ResponseMatchesContract()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty("Setup should have guaranteed at least one comment");
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("11.3 Response is an array")]
    public async Task GetIssueComments_ReturnsArray()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Smoke")]
    [Description("11.4 Pagination with per_page=2 returns at most 2 comments")]
    public async Task GetIssueComments_PaginationWorks()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber), .. PaginationParams(1, 2)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Test]
    [Category("Regression")]
    [Description("11.5 Each comment has valid fields")]
    public async Task GetIssueComments_HasValidFields()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        foreach (var comment in response.Data!)
        {
            comment.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("11.6 All comment IDs are unique")]
    public async Task GetIssueComments_AllIdsAreUnique()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(owner, repo), .. IssueNumberParam(ExistingIssueNumber)]);

        var ids = response.Data!.Select(comment => comment.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Negative")]
    [Description("11.7 Non-existent issue returns 404")]
    public async Task GetIssueComments_NonExistentIssue_ReturnsNotFound()
    {
        var (owner, repo) = ParseRepo();
        var issues = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. RepoParam(owner, repo), .. StateParam(GitHubEndpoints.StateAll)]);
        var maxIssueNumber = issues.Data!.Max(issue => issue.Number);
        var nonExistentIssueNumber = maxIssueNumber + 1;

        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(owner, repo), .. IssueNumberParam(nonExistentIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("11.8 Non-existent repo returns 404")]
    public async Task GetIssueComments_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName), .. IssueNumberParam(1)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
