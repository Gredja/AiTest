using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using Core.Models.GitHub;
using FluentAssertions;
using System.Net;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.GitHub;

public abstract class GitHubE2ETestBase : GitHubTestBase
{
    protected async Task<IssueModelResponse> CreateIssueTrackedAsync(
        CreateIssueModelRequest request, List<int> createdIssueNumbers)
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, request, TestRepoParam());
        if (created.Data is not null)
        {
            createdIssueNumbers.Add(created.Data.Number);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        return created.Data!;
    }

    protected async Task<CommentModelResponse> CreateCommentTrackedAsync(
        CreateCommentModelRequest request, int issueNumber, List<long> createdCommentIds)
    {
        var created = await Post<CreateCommentModelRequest, CommentModelResponse>(
            GitHubEndpoints.RepoIssueComments, request, IssueParams(issueNumber));
        if (created.Data is not null)
        {
            createdCommentIds.Add(created.Data.Id);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        return created.Data!;
    }

    protected async Task WaitUntilIssueInStateListAsync(int issueNumber, string state, bool present)
    {
        var result = await WaitHelper.WaitUntilAsync(
            () => Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
                [.. TestRepoParam(), .. StateParam(state)]),
            response => response.StatusCode == HttpStatusCode.OK
                && (present
                    ? response.Data!.Any(issue => issue.Number == issueNumber)
                    : response.Data!.All(issue => issue.Number != issueNumber)));

        var verb = present ? "appear in" : "leave";
        result.IsSuccess.Should().BeTrue(
            $"issue #{issueNumber} must {verb} ?state={state};" +
            $" elapsed {result.Elapsed}, attempts {result.Attempts}," +
            $" last status {result.LastValue?.StatusCode}");
    }

    protected async Task<IssueModelResponse> PatchIssueStateAsync(int issueNumber, string state)
    {
        var patched = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new UpdateIssueModelRequest { State = state },
            IssueParams(issueNumber));
        patched.ShouldHaveStatusCode(HttpStatusCode.OK);
        return patched.Data!;
    }

    protected async Task AssertIssueStateAsync(int issueNumber, string expectedState, string because)
    {
        var issue = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            IssueParams(issueNumber));
        issue.ShouldHaveStatusCode(HttpStatusCode.OK);
        issue.Data!.State.Should().Be(expectedState, because);
    }

    protected async Task WaitUntilBranchVisibleAsync(string branchName, bool present)
    {
        var result = await WaitHelper.WaitUntilAsync(
            () => Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, TestRepoParam()),
            response => response.StatusCode == HttpStatusCode.OK
                && (present
                    ? response.Data!.Any(item => item.Name == branchName)
                    : response.Data!.All(item => item.Name != branchName)));

        var verb = present ? "appear in" : "leave";
        result.IsSuccess.Should().BeTrue(
            $"branch {branchName} must {verb} the branches list;" +
            $" elapsed {result.Elapsed}, attempts {result.Attempts}," +
            $" last status {result.LastValue?.StatusCode}");
    }

    protected async Task<GitRefModelResponse> CreateBranchTrackedAsync(
        string branchName, string sha, List<string> createdRefs)
    {
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{RefsHeadsPrefix}{branchName}", Sha = sha },
            TestRepoParam());
        if (created.Data is not null)
        {
            createdRefs.Add(branchName);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        return created.Data!;
    }
}
