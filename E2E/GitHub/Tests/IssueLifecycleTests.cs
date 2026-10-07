using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class IssueLifecycleTests : GitHubE2ETestBase
{
    private const int TitleRandomLength = 8;
    private const int BodyRandomLength = 16;

    [Test]
    [Category("Regression")]
    [Description("E2E-1 Issue lifecycle: create → comment → verify comment linked → close → verify state")]
    public async Task IssueLifecycle_Create_Comment_Close()
    {
        var issueRequest = new CreateIssueModelRequest
        {
            Title = $"Lifecycle {DataGenerator.RandomString(TitleRandomLength)}",
            Body = $"Body {DataGenerator.RandomString(BodyRandomLength)}"
        };

        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, issueRequest, TestRepoParam());

        try
        {
            created.ShouldHaveStatusCode(HttpStatusCode.Created);
            created.Data.Should().NotBeNull();
            created.Data!.State.Should().Be(GitHubEndpoints.StateOpen);

            var opened = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
                [.. TestRepoParam(), .. IssueNumberParam(created.Data.Number)]);
            opened.ShouldHaveStatusCode(HttpStatusCode.OK);
            opened.Data!.State.Should().Be(GitHubEndpoints.StateOpen, "freshly created issue must be open");

            var commentRequest = new CreateCommentModelRequest
            {
                Body = $"Comment {DataGenerator.RandomString(BodyRandomLength)}"
            };
            var comment = await Post<CreateCommentModelRequest, CommentModelResponse>(
                GitHubEndpoints.RepoIssueComments, commentRequest,
                [.. TestRepoParam(), .. IssueNumberParam(created.Data.Number)]);

            comment.ShouldHaveStatusCode(HttpStatusCode.Created);
            comment.Data.Should().NotBeNull();
            comment.Data!.ShouldMatchRequest(commentRequest);

            var comments = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
                [.. TestRepoParam(), .. IssueNumberParam(created.Data.Number)]);
            comments.ShouldHaveStatusCode(HttpStatusCode.OK);
            comments.Data.Should().Contain(existing => existing.Id == comment.Data.Id,
                "created comment must be reachable via its issue's comments endpoint");

            var closeRequest = new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed };
            var closed = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
                GitHubEndpoints.RepoIssueById, closeRequest,
                [.. TestRepoParam(), .. IssueNumberParam(created.Data.Number)]);
            closed.ShouldHaveStatusCode(HttpStatusCode.OK);

            var afterClose = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
                [.. TestRepoParam(), .. IssueNumberParam(created.Data.Number)]);
            afterClose.ShouldHaveStatusCode(HttpStatusCode.OK);
            afterClose.Data!.State.Should().Be(GitHubEndpoints.StateClosed, "issue must stay closed after PATCH");
        }
        finally
        {
            if (created.Data is not null)
            {
                await Patch<UpdateIssueModelRequest, IssueModelResponse>(
                    GitHubEndpoints.RepoIssueById,
                    new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed },
                    [.. TestRepoParam(), .. IssueNumberParam(created.Data.Number)]);
            }
        }
    }
}
