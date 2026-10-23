using Core.Config;
using Core.Models.Generic;
using Core.Models.GitHub;
using NUnit.Framework;
using RestSharp;
using System.Net;

namespace Core.Helpers.GitHub;

public class GitHubCleanupHelper : GitHubRequestHelper
{

    public async Task CleanupCommentAsync(long? commentId)
    {
        if (!commentId.HasValue)
        {
            return;
        }

        await RunCleanupAsync(
            () => Delete<object>(GitHubEndpoints.RepoIssueCommentById, CommentParams(commentId.Value, TestRepoParam())),
            $"delete comment {commentId}");
    }

    public async Task CleanupIssueAsync(int? issueNumber)
    {
        if (!issueNumber.HasValue)
        {
            return;
        }

        // REST has no issue-delete endpoint (DELETE → 404, OB §17) — cleanup closes instead
        await RunCleanupAsync(
            () => Patch<UpdateIssueModelRequest, IssueModelResponse>(
                GitHubEndpoints.RepoIssueById,
                new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed },
                IssueParams(issueNumber.Value, TestRepoParam())),
            $"close issue {issueNumber}");
    }

    public async Task CleanupInvitationAsync(long? invitationId)
    {
        if (!invitationId.HasValue)
        {
            return;
        }

        await RunCleanupAsync(
            () => Delete<object>(GitHubEndpoints.RepoInvitationById,
                [.. TestRepoParam(), .. GitHubParamHelper.InvitationIdParam(invitationId.Value)]),
            $"revoke invitation {invitationId}");
    }

    public async Task CleanupReleaseAsync(long? releaseId)
    {
        if (!releaseId.HasValue)
        {
            return;
        }

        await RunCleanupAsync(
            () => Delete<object>(GitHubEndpoints.RepoReleaseById,
                [.. TestRepoParam(), .. GitHubParamHelper.ReleaseIdParam(releaseId.Value)]),
            $"delete release {releaseId}");
    }

    // Accepts any of the forms callers hold ("refs/heads/x", "heads/x", "x", "tags/x",
    // "refs/tags/x") — the endpoint wants "heads/x" or "tags/x"; a wrong form 404s and
    // the NotFound tolerance would silently leak the ref
    public async Task CleanupGitRefAsync(string refPath)
    {
        string path;
        if (refPath.StartsWith(GitHubEndpoints.HeadsPrefix, StringComparison.Ordinal) ||
            refPath.StartsWith(GitHubEndpoints.TagsPrefix, StringComparison.Ordinal))
        {
            path = refPath;
        }
        else if (refPath.StartsWith(GitHubEndpoints.RefsHeadsPrefix, StringComparison.Ordinal))
        {
            path = $"{GitHubEndpoints.HeadsPrefix}{refPath[GitHubEndpoints.RefsHeadsPrefix.Length..]}";
        }
        else if (refPath.StartsWith(GitHubEndpoints.RefsTagsPrefix, StringComparison.Ordinal))
        {
            path = $"{GitHubEndpoints.TagsPrefix}{refPath[GitHubEndpoints.RefsTagsPrefix.Length..]}";
        }
        else
        {
            path = $"{GitHubEndpoints.HeadsPrefix}{refPath}";
        }

        await RunCleanupAsync(
            () => Delete<object>(GitHubEndpoints.RepoGitRefById,
                [.. TestRepoParam(), .. GitHubParamHelper.GitRefParam(path)]),
            $"delete ref {path}");
    }

    public async Task CleanupPullRequestAsync(int? pullNumber, string headBranch, string baseBranch)
    {
        if (pullNumber.HasValue)
        {
            await RunCleanupAsync(
                () => Patch<UpdatePullRequestModelRequest, PullRequestModelResponse>(
                    GitHubEndpoints.RepoPullRequestById,
                    new UpdatePullRequestModelRequest { State = GitHubEndpoints.StateClosed },
                    PullParams(pullNumber.Value, TestRepoParam())),
                $"close pull request {pullNumber}");
        }

        await CleanupGitRefAsync(headBranch);
        await CleanupGitRefAsync(baseBranch);
    }

    private List<RequestDictionaryModel> TestRepoParam()
    {
        var (owner, repo) = GitHubParamHelper.ParseRepo(RepoFullName);

        return GitHubParamHelper.RepoParam(owner, repo);
    }

    protected virtual string RepoFullName => TestConfig.GitHubTestRepo;

    private static List<RequestDictionaryModel> PullParams(int pullNumber, List<RequestDictionaryModel> testRepo) =>
        [.. testRepo, .. GitHubParamHelper.PullRequestNumberParam(pullNumber)];

    private static List<RequestDictionaryModel> CommentParams(long commentId, List<RequestDictionaryModel> testRepo) =>
        [.. testRepo, .. GitHubParamHelper.CommentIdParam(commentId)];

    private static List<RequestDictionaryModel> IssueParams(int issueNumber, List<RequestDictionaryModel> testRepo) =>
        [.. testRepo, .. GitHubParamHelper.IssueNumberParam(issueNumber)];

    private async Task RunCleanupAsync<T>(Func<Task<T>> cleanup, string subject)
        where T : RestResponse
    {
        try
        {
            var response = await cleanup();
            if (!response.IsSuccessful && response.StatusCode != HttpStatusCode.NotFound)
            {
                TestContext.Progress.WriteLine($"Warning: {subject}: HTTP {(int)response.StatusCode}");
            }
        }
        catch (HttpRequestException exception)
        {
            TestContext.Progress.WriteLine($"Warning: {subject} failed: {exception.Message}");
        }
        catch (OperationCanceledException exception)
        {
            TestContext.Progress.WriteLine($"Warning: {subject} cancelled: {exception.Message}");
        }
    }
}
