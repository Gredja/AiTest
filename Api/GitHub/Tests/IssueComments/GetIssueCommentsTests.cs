using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;
using static Core.Helpers.ParamHelper;

namespace Api.GitHub.IssueComments;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetIssueCommentsTests : GitHubTestBase
{
    private const int ExistingIssueNumber = 5;
    private const int FirstIssueNumber = 1;
    private const string TestComment = "Test comment for contract check";

    private long? _createdCommentId;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        if (response.Data!.Count == 0)
        {
            var create = await Post<CreateCommentModelRequest, CommentModelResponse>(
                GitHubEndpoints.RepoIssueComments,
                new CreateCommentModelRequest { Body = TestComment },
                [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);
            _createdCommentId = create.Data!.Id;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdCommentId.HasValue)
        {
            try
            {
                var delete = await Delete<object>(
                    GitHubEndpoints.RepoIssueCommentById,
                    [.. TestRepoParam(), .. CommentIdParam(_createdCommentId.Value)]);

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
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("11.2 Response matches expected contract")]
    public async Task GetIssueComments_ResponseMatchesContract()
    {
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

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
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Smoke")]
    [Description("11.4 Pagination with per_page=2 returns at most 2 comments")]
    public async Task GetIssueComments_PaginationWorks()
    {
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber), .. PaginationParams(FirstPage, SmallPageSize)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(SmallPageSize);
    }

    [Test]
    [Category("Regression")]
    [Description("11.5 Each comment has valid fields")]
    public async Task GetIssueComments_HasValidFields()
    {
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

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
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        var ids = response.Data!.Select(comment => comment.Id);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Negative")]
    [Description("11.7 Non-existent issue returns 404")]
    public async Task GetIssueComments_NonExistentIssue_ReturnsNotFound()
    {
        var issues = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        var maxIssueNumber = issues.Data!.Max(issue => issue.Number);
        var nonExistentIssueNumber = maxIssueNumber + 1;

        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. TestRepoParam(), .. IssueNumberParam(nonExistentIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("11.8 Non-existent repo returns 404")]
    public async Task GetIssueComments_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName), .. IssueNumberParam(FirstIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    private const string InvalidQueryValue = "abc";

    [Test]
    [Category("Negative")]
    [Description("11.9 since=abc returns 422")]
    public async Task GetIssueComments_InvalidSince_Returns422()
    {
        var response = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber), Query(SinceParamKey, InvalidQueryValue)]);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }
}
