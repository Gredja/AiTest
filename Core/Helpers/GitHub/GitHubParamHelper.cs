using Core.Config;
using Core.Models.Generic;
using JsonFields = Core.Models.GitHub.JsonFields;

namespace Core.Helpers.GitHub;

public static class GitHubParamHelper
{
    private const string AcceptHeader = "Accept";
    public static List<RequestDictionaryModel> RepoParam(string owner, string repo) =>
        [
            ParamHelper.UrlSegment(GitHubEndpoints.OwnerPlaceholder, owner),
            ParamHelper.UrlSegment(GitHubEndpoints.RepoPlaceholder, repo)
        ];

    public static (string Owner, string Repo) ParseRepo(string fullName)
    {
        var parts = fullName.Split('/');

        return (parts[0], parts[1]);
    }

    public static List<RequestDictionaryModel> IssueNumberParam(int number) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.IssueNumberPlaceholder, number)];

    public static List<RequestDictionaryModel> IssueNumberParam(string number) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.IssueNumberPlaceholder, number)];

    public static List<RequestDictionaryModel> PullRequestNumberParam(int number) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.PullNumberPlaceholder, number)];

    public static List<RequestDictionaryModel> CommentIdParam(long id) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.CommentIdPlaceholder, id)];

    public static List<RequestDictionaryModel> BranchNameParam(string branch) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.BranchPlaceholder, branch)];

    public static List<RequestDictionaryModel> PaginationParams(int page, int perPage) =>
        [
            ParamHelper.Query(JsonFields.Page, page),
            .. PerPageParam(perPage)
        ];

    public static List<RequestDictionaryModel> PerPageParam(object perPage) =>
        [ParamHelper.Query(JsonFields.PerPage, perPage)];

    public static List<RequestDictionaryModel> MercyPreviewParam() =>
        [ParamHelper.Header(AcceptHeader, GitHubEndpoints.MercyPreviewAccept)];

    public static List<RequestDictionaryModel> GitRefParam(string refPath) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.RefPlaceholder, refPath)];

    public static List<RequestDictionaryModel> GitCommitParam(string commitSha) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.CommitShaPlaceholder, commitSha)];

    public static List<RequestDictionaryModel> UsernameParam(string username) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.UsernamePlaceholder, username)];

    public static List<RequestDictionaryModel> InvitationIdParam(long id) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.InvitationIdPlaceholder, id)];

    public static List<RequestDictionaryModel> ReleaseIdParam(long id) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.ReleaseIdPlaceholder, id)];

    public static List<RequestDictionaryModel> ReleaseIdParam(string id) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.ReleaseIdPlaceholder, id)];

    public static List<RequestDictionaryModel> ReleaseTagParam(string tag) =>
        [ParamHelper.UrlSegment(GitHubEndpoints.ReleaseTagPlaceholder, tag)];

    public static List<RequestDictionaryModel> SearchQueryParam(string query) =>
        [ParamHelper.Query(JsonFields.Q, query)];

    public static List<RequestDictionaryModel> StateParam(string state) =>
        [ParamHelper.Query(JsonFields.State, state)];
}
