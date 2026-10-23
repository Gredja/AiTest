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
public class CommentChainTests : GitHubE2ETestBase
{
    private const int ChainLength = 3;

    private readonly List<long> _createdCommentIds = [];
    private readonly List<int> _createdIssueNumbers = [];

    [Test]
    [Category("Regression")]
    [Description("E2E-3 Comment chain: create issue → add comments → verify count and ordering")]
    public async Task CommentChain_Create_AddComments_VerifyCountAndOrdering()
    {
        var issue = await CreateIssueTrackedAsync(
            new CreateIssueModelRequest { Title = $"Chain {DataGenerator.RandomString(TitleRandomLength)}" },
            _createdIssueNumbers);
        var issueNumber = issue.Number;

        var createdBodies = new List<string>();
        for (var index = 0; index < ChainLength; index++)
        {
            var body = $"Chain {index} {DataGenerator.RandomString(BodyRandomLength)}";
            await CreateCommentTrackedAsync(
                new CreateCommentModelRequest { Body = body },
                issueNumber,
                _createdCommentIds);
            createdBodies.Add(body);
        }

        var comments = await Get<List<CommentModelResponse>>(GitHubEndpoints.RepoIssueComments,
            IssueParams(issueNumber));
        comments.ShouldHaveStatusCode(HttpStatusCode.OK);
        comments.Data.Should().HaveCount(ChainLength, "fresh issue must contain exactly the created comments");
        comments.Data!.Select(comment => comment.Body).Should().Equal(createdBodies,
            "comments must come back in creation order (created_at ascending, OB §8)");
        comments.Data.Select(comment => comment.Id).Should().OnlyHaveUniqueItems();
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
