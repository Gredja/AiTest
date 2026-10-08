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
public class DeleteCommentTests : GitHubTestBase
{
    private const int TargetIssueNumber = 5;
    private const int BodyRandomLength = 16;
    private const long NonExistentCommentId = 0;

    private static readonly CreateCommentModelRequest _testComment = new()
    {
        Body = $"Cleanup probe {DataGenerator.RandomString(BodyRandomLength)}"
    };

    private readonly List<long> _createdCommentIds = [];

    [Test]
    [Category("HealthCheck")]
    [Description("20.1 DELETE comment returns 204 No Content")]
    public async Task DeleteComment_ReturnsNoContent()
    {
        var created = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, _testComment, IssueParams(TargetIssueNumber));
        if (created.Data is not null)
        {
            _createdCommentIds.Add(created.Data.Id);
        }
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();

        var deleted = await Delete<object>(
            GitHubEndpoints.RepoIssueCommentById, CommentParams(created.Data!.Id));

        deleted.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Test]
    [Category("Regression")]
    [Description("20.2 Deleted comment is gone (GET by id returns 404)")]
    public async Task DeleteComment_GoneAfterDelete()
    {
        var created = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, _testComment, IssueParams(TargetIssueNumber));
        if (created.Data is not null)
        {
            _createdCommentIds.Add(created.Data.Id);
        }
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();

        var deleted = await Delete<object>(
            GitHubEndpoints.RepoIssueCommentById, CommentParams(created.Data!.Id));
        deleted.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        var readBack = await Get<CommentModelResponse>(GitHubEndpoints.RepoIssueCommentById,
            CommentParams(created.Data.Id));
        readBack.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("20.3 Comment id 0 returns 404")]
    public async Task DeleteComment_ZeroId_ReturnsNotFound()
    {
        var response = await Delete<object>(
            GitHubEndpoints.RepoIssueCommentById, CommentParams(NonExistentCommentId));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("20.4 DELETE without auth returns 401")]
    public async Task DeleteComment_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoIssueCommentById, Method.Delete);
        AddParams(request, CommentParams(NonExistentCommentId));

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("20.5 Invalid token returns 401")]
    public async Task DeleteComment_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoIssueCommentById, Method.Delete);
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        AddParams(request, CommentParams(NonExistentCommentId));

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("20.6 Non-existent repo returns 404")]
    public async Task DeleteComment_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Delete<object>(
            GitHubEndpoints.RepoIssueCommentById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. CommentIdParam(NonExistentCommentId)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("20.7 Non-existent owner returns 404")]
    public async Task DeleteComment_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Delete<object>(
            GitHubEndpoints.RepoIssueCommentById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, repo),
             .. CommentIdParam(NonExistentCommentId)]);

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
