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

    protected async Task CleanupGitRefAsync(string refPath)
    {
        await RunCleanupAsync(
            () => Delete<object>(GitHubEndpoints.RepoGitRefById, GitHubParamHelper.GitRefParam(refPath)),
            $"delete ref {refPath}");
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
