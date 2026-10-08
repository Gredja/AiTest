using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.IssueComments;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class CreateCommentTests : GitHubTestBase
{
    private const int TargetIssueNumber = 5;
    private const int BodyRandomLength = 16;

    private static readonly CreateCommentModelRequest _testComment = new()
    {
        Body = $"Comment {DataGenerator.RandomString(BodyRandomLength)}"
    };

    private readonly List<long> _createdCommentIds = [];

    [Test]
    [Category("HealthCheck")]
    [Description("18.1 POST comment returns 201 Created")]
    public async Task CreateComment_ReturnsCreated()
    {
        var created = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, _testComment, IssueParams(TargetIssueNumber));
        if (created.Data is not null)
        {
            _createdCommentIds.Add(created.Data.Id);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();
        created.Data!.Id.Should().BeGreaterThan(0, "created comment must get a server id");
    }

    [Test]
    [Category("Smoke")]
    [Description("18.2 Response matches request")]
    public async Task CreateComment_MatchesRequest()
    {
        var created = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, _testComment, IssueParams(TargetIssueNumber));
        if (created.Data is not null)
        {
            _createdCommentIds.Add(created.Data.Id);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();
        created.Data!.ShouldMatchRequest(_testComment);
    }

    [Test]
    [Category("Regression")]
    [Description("18.3 Created comment is visible in the issue comments list")]
    public async Task CreateComment_VisibleInList()
    {
        var baseline = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments, IssueParams(TargetIssueNumber));
        baseline.ShouldHaveStatusCode(HttpStatusCode.OK);
        var baselineCount = baseline.Data!.Count;

        var created = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, _testComment, IssueParams(TargetIssueNumber));
        if (created.Data is not null)
        {
            _createdCommentIds.Add(created.Data.Id);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();

        var current = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments, IssueParams(TargetIssueNumber));
        current.ShouldHaveStatusCode(HttpStatusCode.OK);
        current.Data.Should().HaveCount(baselineCount + 1, "the list must grow by exactly one comment");
        current.Data!.Select(comment => comment.Id)
            .Should().Contain(created.Data!.Id, "created comment must be reachable via the list");
    }

    [Test]
    [Category("Performance")]
    [Description("18.4 Created comment becomes visible within max response time")]
    public async Task CreateComment_RecordVisibleWithinTimeLimit()
    {
        var created = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, _testComment, IssueParams(TargetIssueNumber));
        if (created.Data is not null)
        {
            _createdCommentIds.Add(created.Data.Id);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();

        var result = await WaitHelper.WaitUntilAsync(
            () => Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments, IssueParams(TargetIssueNumber)),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data is not null
                && response.Data.Any(comment => comment.Id == created.Data!.Id),
            timeout: TimeSpan.FromMilliseconds(TestConfig.MaxResponseTimeMs));

        result.IsSuccess.Should().BeTrue(
            $"created comment should be visible within {TestConfig.MaxResponseTimeMs} ms;" +
            $" waited {result.Elapsed}, attempts {result.Attempts}," +
            $" last status {result.LastValue?.StatusCode}");
    }

    [Test]
    [Category("Smoke")]
    [Description("18.5 Content-Type is application/json")]
    public async Task CreateComment_ContentTypeIsJson()
    {
        var created = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, _testComment, IssueParams(TargetIssueNumber));
        if (created.Data is not null)
        {
            _createdCommentIds.Add(created.Data.Id);
        }

        created.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("18.6 POST without auth returns 401")]
    public async Task CreateComment_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoIssueComments, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddJsonBody(_testComment);
        AddParams(request, IssueParams(TargetIssueNumber));

        var response = await Client.ExecuteAsync<CommentModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("18.7 Invalid token returns 401")]
    public async Task CreateComment_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoIssueComments, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        request.AddJsonBody(_testComment);
        AddParams(request, IssueParams(TargetIssueNumber));

        var response = await Client.ExecuteAsync<CommentModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("18.8 POST without body returns 422")]
    public async Task CreateComment_MissingBody_Returns422()
    {
        var response = await Post<Dictionary<string, object>, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, new Dictionary<string, object>(), IssueParams(TargetIssueNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("18.9 Non-existent issue returns 404")]
    public async Task CreateComment_NonExistentIssue_ReturnsNotFound()
    {
        var issues = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        issues.ShouldHaveStatusCode(HttpStatusCode.OK);
        issues.Data.Should().NotBeEmpty("repo must keep data — Entry Criteria, documentation/GitHubTestingStructure.md");
        var nonExistentIssueNumber = issues.Data!.Max(issue => issue.Number) + GitHubEndpoints.NonExistentIdOffset;

        var response = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, _testComment,
            [.. TestRepoParam(), .. IssueNumberParam(nonExistentIssueNumber)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("18.10 Non-existent repo returns 404")]
    public async Task CreateComment_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, _testComment,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var commentId in _createdCommentIds)
        {
            await CleanupCommentAsync(commentId);
        }
    }
}
