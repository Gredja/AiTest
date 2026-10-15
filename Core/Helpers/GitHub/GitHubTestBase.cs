using Core.Config;
using Core.Models.Generic;
using Core.Models.GitHub;
using RestSharp;
using System.Net;
using System.Text.RegularExpressions;
using static Core.Helpers.TestDataConstants;

namespace Core.Helpers.GitHub;

public abstract class GitHubTestBase : GitHubRequestHelper
{
    protected const int FirstPage = 1;
    protected const int DefaultPageSize = 5;
    protected const int SmallPageSize = 2;
    protected const string InvalidToken = "ghp_invalidtoken123";
    protected const string InvalidAuthorization = $"Bearer {InvalidToken}";

    // GitHub topic naming: lowercase letters, digits and hyphens (OB §14), e.g. "api-v2"
    protected static readonly Regex TopicNamePattern = new("^[a-z0-9-]+$", RegexOptions.Compiled);

    private const string ScratchFileName = "audit-scratch.txt";
    private const string FileMode644 = "100644";
    private const string BlobType = "blob";
    private const string ScratchBranchPrefix = "audit-";

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

    protected static string RateLimitHeaderValue(RestResponse response, string headerName) =>
        response.Headers?
            .FirstOrDefault(header => header.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase))
            ?.Value ?? string.Empty;

    protected static List<RequestDictionaryModel> IssueParams(int issueNumber) =>
        [.. TestRepoParam(), .. GitHubParamHelper.IssueNumberParam(issueNumber)];

    private readonly GitHubCleanupHelper _cleanup = new();

    protected Task CleanupCommentAsync(long? commentId) => _cleanup.CleanupCommentAsync(commentId);

    protected Task CleanupIssueAsync(int? issueNumber) => _cleanup.CleanupIssueAsync(issueNumber);

    protected Task CleanupInvitationAsync(long? invitationId) => _cleanup.CleanupInvitationAsync(invitationId);

    protected Task CleanupGitRefAsync(string refPath) => _cleanup.CleanupGitRefAsync(refPath);

    // Scratch PR pipeline: changed blob → tree → commit → branch → PR. A same-tree commit gives
    // "No commits between" (OB §21) and an empty files list for the dynamic-max PR fixtures —
    // the new file guarantees both divergence and ≥1 changed file.
    protected async Task<(int Number, string HeadBranch, string BaseBranch)> CreateScratchPullRequestAsync(string title)
    {
        var headBranch = $"{ScratchBranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var baseBranch = $"{ScratchBranchPrefix}base-{DataGenerator.RandomString(RandomNameLength)}";

        var mainSha = await ResolveDefaultBranchShaAsync();
        var commitSha = await CreateScratchCommitAsync(headBranch, mainSha);

        // A merge must never target main (merged scratch data would change the repo — the
        // "create then remove, never keep test records" rule): every PR gets its own base
        // branch, deleted together with the head. Orphan blob/tree/commit objects are
        // unreachable and harmless; branch and PR are cleaned on any partial failure.
        PullRequestModelResponse? createdPull = null;
        try
        {
            await CreateScratchRefsAsync(headBranch, baseBranch, commitSha, mainSha);
            createdPull = await CreatePullRequestForScratchAsync(title, headBranch, baseBranch);

            return (createdPull.Number, headBranch, baseBranch);
        }
        catch (Exception)
        {
            await CleanupPullRequestAsync(createdPull?.Number, headBranch, baseBranch);
            throw;
        }
    }

    private async Task<string> ResolveDefaultBranchShaAsync()
    {
        var mainBranch = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. GitHubParamHelper.BranchNameParam(GitHubEndpoints.DefaultBranch)]);
        mainBranch.ShouldHaveStatusCode(HttpStatusCode.OK);

        return mainBranch.Data!.Commit.Sha;
    }

    private async Task<string> CreateScratchCommitAsync(string headBranch, string mainSha)
    {
        var mainCommit = await Get<GitCommitModelResponse>(GitHubEndpoints.RepoGitCommitsById,
            [.. TestRepoParam(), .. GitHubParamHelper.GitCommitParam(mainSha)]);
        mainCommit.ShouldHaveStatusCode(HttpStatusCode.OK);

        var blobSha = await CreateScratchBlobAsync(headBranch);
        var treeSha = await CreateScratchTreeAsync(headBranch, mainCommit.Data!.Tree.Sha, blobSha);

        var commit = await Post<CreateGitCommitModelRequest, GitCommitModelResponse>(
            GitHubEndpoints.RepoGitCommits,
            new CreateGitCommitModelRequest
            {
                Message = $"scratch {headBranch}",
                Tree = treeSha,
                Parents = [mainSha]
            },
            TestRepoParam());
        commit.ShouldHaveStatusCode(HttpStatusCode.Created);

        return commit.Data!.Sha;
    }

    private async Task<string> CreateScratchBlobAsync(string headBranch)
    {
        var blob = await Post<CreateGitBlobModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs,
            new CreateGitBlobModelRequest { Content = $"scratch {headBranch}" },
            TestRepoParam());
        blob.ShouldHaveStatusCode(HttpStatusCode.Created);

        return blob.Data!.Sha;
    }

    private async Task<string> CreateScratchTreeAsync(string headBranch, string baseTreeSha, string blobSha)
    {
        var tree = await Post<CreateGitTreeModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitTrees,
            new CreateGitTreeModelRequest
            {
                BaseTree = baseTreeSha,
                Tree =
                [
                    new GitTreeEntry
                    {
                        Path = ScratchFileName,
                        Mode = FileMode644,
                        Type = BlobType,
                        Sha = blobSha
                    }
                ]
            },
            TestRepoParam());
        tree.ShouldHaveStatusCode(HttpStatusCode.Created);

        return tree.Data!.Sha;
    }

    private async Task CreateScratchRefsAsync(string headBranch, string baseBranch, string commitSha, string mainSha)
    {
        var head = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{GitHubEndpoints.RefsHeadsPrefix}{headBranch}", Sha = commitSha },
            TestRepoParam());
        head.ShouldHaveStatusCode(HttpStatusCode.Created);

        var baseRef = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{GitHubEndpoints.RefsHeadsPrefix}{baseBranch}", Sha = mainSha },
            TestRepoParam());
        baseRef.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    private async Task<PullRequestModelResponse> CreatePullRequestForScratchAsync(string title, string headBranch, string baseBranch)
    {
        var pullRequest = await Post<CreatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequests,
            new CreatePullRequestModelRequest
            {
                Title = title,
                Head = headBranch,
                Base = baseBranch
            },
            TestRepoParam());
        pullRequest.ShouldHaveStatusCode(HttpStatusCode.Created);

        return pullRequest.Data
            ?? throw new InvalidOperationException("PR create returned 201 without a body");
    }

    protected Task CleanupPullRequestAsync(int? pullNumber, string headBranch, string baseBranch) =>
        _cleanup.CleanupPullRequestAsync(pullNumber, headBranch, baseBranch);

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