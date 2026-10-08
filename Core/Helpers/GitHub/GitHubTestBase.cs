using Core.Config;
using Core.Models.Generic;
using Core.Models.GitHub;
using NUnit.Framework;
using RestSharp;

namespace Core.Helpers.GitHub;

public abstract class GitHubTestBase : GitHubRequestHelper
{
    protected const int FirstPage = 1;
    protected const int DefaultPageSize = 5;
    protected const int SmallPageSize = 2;
    protected const string InvalidToken = "ghp_invalidtoken123";
    protected const string InvalidAuthorization = $"Bearer {InvalidToken}";
    protected const string SinceParamKey = "since";

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

        try
        {
            var cleanup = await Delete<object>(
                GitHubEndpoints.RepoIssueCommentById, CommentParams(commentId.Value));

            if (!cleanup.IsSuccessful)
            {
                TestContext.Progress.WriteLine(
                    $"Warning: comment {commentId} not deleted: HTTP {(int)cleanup.StatusCode}");
            }
        }
        catch (HttpRequestException exception)
        {
            TestContext.Progress.WriteLine(
                $"Warning: failed to delete comment {commentId}: {exception.Message}");
        }
    }

    protected async Task CleanupIssueAsync(int? issueNumber)
    {
        if (!issueNumber.HasValue)
        {
            return;
        }

        try
        {
            // REST has no issue-delete endpoint (DELETE → 404, OB §17) — cleanup closes instead
            var cleanup = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
                GitHubEndpoints.RepoIssueById,
                new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed },
                IssueParams(issueNumber.Value));

            if (!cleanup.IsSuccessful)
            {
                TestContext.Progress.WriteLine(
                    $"Warning: issue {issueNumber} not closed: HTTP {(int)cleanup.StatusCode}");
            }
        }
        catch (HttpRequestException exception)
        {
            TestContext.Progress.WriteLine(
                $"Warning: failed to close issue {issueNumber}: {exception.Message}");
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
