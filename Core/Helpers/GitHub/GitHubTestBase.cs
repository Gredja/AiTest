using Core.Config;
using Core.Models.Generic;
using Core.Models.GitHub;
using NUnit.Framework;
using RestSharp;
using System.Net;

namespace Core.Helpers.GitHub;

public abstract class GitHubTestBase : GitHubRequestHelper
{
    protected const int FirstPage = 1;
    protected const int DefaultPageSize = 5;
    protected const int SmallPageSize = 2;
    protected const string InvalidToken = "ghp_invalidtoken123";
    protected const string InvalidAuthorization = $"Bearer {InvalidToken}";
    protected const string SinceParamKey = "since";

    private const string ScratchFileName = "audit-scratch.txt";
    private const string FileMode644 = "100644";
    private const string BlobType = "blob";
    private const string RefsPrefix = "refs/";
    private const string HeadsPrefix = "heads/";

    protected static List<RequestDictionaryModel> TestRepoParam()
    {
        var (owner, repo) = ParseRepo();

        return GitHubParamHelper.RepoParam(owner, repo);
    }

    protected static List<RequestDictionaryModel> PullParams(int pullNumber) =>
        [.. TestRepoParam(), .. GitHubParamHelper.PullRequestNumberParam(pullNumber)];

    protected static List<RequestDictionaryModel> MercyPreviewRepoParams() =>
        [.. TestRepoParam(), .. GitHubParamHelper.MercyPreviewParam()];

    protected static List<RequestDictionaryModel> CommentParams(long commentId) =>
        [.. TestRepoParam(), .. GitHubParamHelper.CommentIdParam(commentId)];

    protected static List<RequestDictionaryModel> IssueParams(int issueNumber) =>
        [.. TestRepoParam(), .. GitHubParamHelper.IssueNumberParam(issueNumber)];

    protected async Task CleanupCommentAsync(long? commentId)
    {
        if (!commentId.HasValue)
        {
            return;
        }

        await RunCleanupAsync(
            () => Delete<object>(GitHubEndpoints.RepoIssueCommentById, CommentParams(commentId.Value)),
            $"delete comment {commentId}");
    }

    protected async Task CleanupIssueAsync(int? issueNumber)
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

    // Accepts any of the three forms callers hold ("refs/heads/x", "heads/x", "x") —
    // the endpoint wants exactly "heads/x"; a wrong form 404s and the NotFound
    // tolerance would silently leak the branch
    protected async Task CleanupGitRefAsync(string refPath)
    {
        var path = refPath.StartsWith(RefsPrefix, StringComparison.Ordinal)
            ? refPath[RefsPrefix.Length..]
            : refPath;
        if (!path.StartsWith(HeadsPrefix, StringComparison.Ordinal))
        {
            path = $"{HeadsPrefix}{path}";
        }

        await RunCleanupAsync(
            () => Delete<object>(GitHubEndpoints.RepoGitRefById,
                [.. TestRepoParam(), .. GitHubParamHelper.GitRefParam(path)]),
            $"delete ref {path}");
    }

    // Scratch PR pipeline: changed blob → tree → commit → branch → PR. A same-tree commit gives
    // "No commits between" (OB §21) and an empty files list for the dynamic-max PR fixtures —
    // the new file guarantees both divergence and ≥1 changed file.
    protected async Task<(int Number, string BranchName)> CreateScratchPullRequestAsync(string title)
    {
        var branchName = $"audit-{DataGenerator.RandomString(8)}";

        var mainBranch = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. GitHubParamHelper.BranchNameParam(GitHubEndpoints.DefaultBranch)]);
        mainBranch.ShouldHaveStatusCode(HttpStatusCode.OK);
        var mainSha = mainBranch.Data!.Commit.Sha;

        var mainCommit = await Get<GitCommitModelResponse>(GitHubEndpoints.RepoGitCommitsById,
            [.. TestRepoParam(), .. GitHubParamHelper.GitCommitParam(mainSha)]);
        mainCommit.ShouldHaveStatusCode(HttpStatusCode.OK);

        var blob = await Post<CreateGitBlobModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs,
            new CreateGitBlobModelRequest { Content = $"scratch {branchName}" },
            TestRepoParam());
        blob.ShouldHaveStatusCode(HttpStatusCode.Created);

        var tree = await Post<CreateGitTreeModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitTrees,
            new CreateGitTreeModelRequest
            {
                BaseTree = mainCommit.Data!.Tree.Sha,
                Tree =
                [
                    new GitTreeEntry
                    {
                        Path = ScratchFileName,
                        Mode = FileMode644,
                        Type = BlobType,
                        Sha = blob.Data!.Sha
                    }
                ]
            },
            TestRepoParam());
        tree.ShouldHaveStatusCode(HttpStatusCode.Created);

        var commit = await Post<CreateGitCommitModelRequest, GitCommitModelResponse>(
            GitHubEndpoints.RepoGitCommits,
            new CreateGitCommitModelRequest
            {
                Message = $"scratch {branchName}",
                Tree = tree.Data!.Sha,
                Parents = [mainSha]
            },
            TestRepoParam());
        commit.ShouldHaveStatusCode(HttpStatusCode.Created);

        // Orphan blob/tree/commit objects are harmless (unreachable); branch and PR are the
        // remote-visible resources — clean them on any partial failure before rethrowing
        try
        {
            var branch = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
                GitHubEndpoints.RepoGitRefs,
                new CreateGitRefModelRequest { Ref = $"refs/heads/{branchName}", Sha = commit.Data!.Sha },
                TestRepoParam());
            branch.ShouldHaveStatusCode(HttpStatusCode.Created);

            var pullRequest = await Post<CreatePullRequestModelRequest, PullRequestModelResponse>(
                GitHubEndpoints.RepoPullRequests,
                new CreatePullRequestModelRequest
                {
                    Title = title,
                    Head = branchName,
                    Base = GitHubEndpoints.DefaultBranch
                },
                TestRepoParam());
            pullRequest.ShouldHaveStatusCode(HttpStatusCode.Created);

            return (pullRequest.Data!.Number, branchName);
        }
        catch
        {
            await CleanupGitRefAsync(branchName);
            throw;
        }
    }

    protected async Task CleanupPullRequestAsync(int? pullNumber, string branchName)
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

        await CleanupGitRefAsync(branchName);
    }

    // One cleanup core for every entity: warn, never fail the run (Rules/test-practices.md → E2E cleanup)
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

    protected static (string Owner, string Repo) ParseRepo() => ParseRepo(TestConfig.GitHubTestRepo);

    protected async Task<RestResponse> ExecuteWithAuthorization(
        string endpoint, string? authorization, List<RequestDictionaryModel>? additionalParams = null)
    {
        var request = new RestRequest(endpoint, Method.Get) { RequestFormat = DataFormat.Json };

        if (authorization is not null)
        {
            request.AddHeader(AuthorizationHeader, authorization);
        }

        if (additionalParams is not null)
        {
            AddParams(request, additionalParams);
        }

        return await Client.ExecuteAsync(request);
    }

    private static (string Owner, string Repo) ParseRepo(string fullName)
    {
        var parts = fullName.Split('/');

        return (parts[0], parts[1]);
    }
}
