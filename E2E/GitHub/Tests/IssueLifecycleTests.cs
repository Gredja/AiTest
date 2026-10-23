using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using static Core.Helpers.TestDataConstants;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class IssueLifecycleTests : GitHubE2ETestBase
{
    private readonly List<int> _createdIssueNumbers = [];
    private readonly List<long> _createdCommentIds = [];

    [Test]
    [Category("Regression")]
    [Description("E2E-1 Issue lifecycle: create → comment → verify comment linked → close → verify state")]
    public async Task IssueLifecycle_Create_Comment_Close()
    {
        var created = await CreateIssueTrackedAsync(
            new CreateIssueModelRequest
            {
                Title = $"Lifecycle {DataGenerator.RandomString(TitleRandomLength)}",
                Body = $"Body {DataGenerator.RandomString(BodyRandomLength)}"
            },
            _createdIssueNumbers);
        created.State.Should().Be(GitHubEndpoints.StateOpen);
        await AssertIssueStateAsync(created.Number, GitHubEndpoints.StateOpen, "freshly created issue must be open");

        var commentRequest = new CreateCommentModelRequest
        {
            Body = $"Comment {DataGenerator.RandomString(BodyRandomLength)}"
        };
        var comment = await CreateCommentTrackedAsync(commentRequest, created.Number, _createdCommentIds);
        comment.ShouldMatchRequest(commentRequest);

        var comments = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            IssueParams(created.Number));
        comments.ShouldHaveStatusCode(HttpStatusCode.OK);
        comments.Data.Should().Contain(existing => existing.Id == comment.Id,
            "created comment must be reachable via its issue's comments endpoint");

        await PatchIssueStateAsync(created.Number, GitHubEndpoints.StateClosed);
        await AssertIssueStateAsync(created.Number, GitHubEndpoints.StateClosed, "issue must stay closed after PATCH");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var commentId in _createdCommentIds)
        {
            await CleanupCommentAsync(commentId);
        }

        foreach (var issueNumber in _createdIssueNumbers)
        {
            await CleanupIssueAsync(issueNumber);
        }
    }
}
