using Core.Config;
using Core.Models.Generic;
using Core.Models.GitHub;
using NUnit.Framework;
using RestSharp;
using System.Net;

namespace Core.Helpers.GitHub;

// Shared cleanup core for Api/E2E (via GitHubTestBase delegation) and Ui (composition).
// Warning-only: never fails the run (Rules/test-practices.md → E2E cleanup).
public class GitHubCleanupHelper : GitHubRequestHelper
{
    private const string RefsHeadsPrefix = "refs/heads/";
    private const string HeadsPrefix = "heads/";

    public async Task CleanupCommentAsync(long? commentId)
    {
        if (!commentId.HasValue)
        {
            return;
        }

        await RunCleanupAsync(
            () => Delete<object>(GitHubEndpoints.RepoIssueCommentById, CommentParams(commentId.Value)),
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
                IssueParams(issueNumber.Value)),
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

    // Accepts any of the three forms callers hold ("refs/heads/x", "heads/x", "x") —
    // the endpoint wants exactly "heads/x"; a wrong form 404s and the NotFound
    // tolerance would silently leak the branch
    public async Task CleanupGitRefAsync(string refPath)
    {
        string path;
        if (refPath.StartsWith(RefsHeadsPrefix, StringComparison.Ordinal))
        {
            path = $"{HeadsPrefix}{refPath[RefsHeadsPrefix.Length..]}";
        }
        else if (refPath.StartsWith(HeadsPrefix, StringComparison.Ordinal))
        {
            path = refPath;
        }
        else
        {
            path = $"{HeadsPrefix}{refPath}";
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
                    PullParams(pullNumber.Value)),
                $"close pull request {pullNumber}");
        }

        await CleanupGitRefAsync(headBranch);
        await CleanupGitRefAsync(baseBranch);
    }

    private static List<RequestDictionaryModel> TestRepoParam()
    {
        var (owner, repo) = ParseRepo(TestConfig.GitHubTestRepo);

        return GitHubParamHelper.RepoParam(owner, repo);
    }

    private static List<RequestDictionaryModel> PullParams(int pullNumber) =>
        [.. TestRepoParam(), .. GitHubParamHelper.PullRequestNumberParam(pullNumber)];

    private static List<RequestDictionaryModel> CommentParams(long commentId) =>
        [.. TestRepoParam(), .. GitHubParamHelper.CommentIdParam(commentId)];

    private static List<RequestDictionaryModel> IssueParams(int issueNumber) =>
        [.. TestRepoParam(), .. GitHubParamHelper.IssueNumberParam(issueNumber)];

    private static (string Owner, string Repo) ParseRepo(string fullName)
    {
        var parts = fullName.Split('/');

        return (parts[0], parts[1]);
    }

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
    }
}
