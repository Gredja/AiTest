using Core.Config;
using Core.Models.Generic;
using JsonFields = Core.Models.GitHub.JsonFields;

namespace Core.Helpers.GitHub;

public static class GitHubParamHelper
{
    private const string AcceptHeader = "Accept";
    public static List<RequestDictionaryModel> RepoParam(string owner, string repo) =>
        [
            ParamHelper.UrlSegment("owner", owner),
            ParamHelper.UrlSegment("repo", repo)
        ];

    public static List<RequestDictionaryModel> IssueNumberParam(int number) =>
        [ParamHelper.UrlSegment("issue_number", number)];

    public static List<RequestDictionaryModel> PullRequestNumberParam(int number) =>
        [ParamHelper.UrlSegment("pull_number", number)];

    public static List<RequestDictionaryModel> CommentIdParam(long id) =>
        [ParamHelper.UrlSegment("comment_id", id)];

    public static List<RequestDictionaryModel> BranchNameParam(string branch) =>
        [ParamHelper.UrlSegment("branch", branch)];

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
        [ParamHelper.UrlSegment("ref", refPath)];

    public static List<RequestDictionaryModel> GitCommitParam(string commitSha) =>
        [ParamHelper.UrlSegment("commit_sha", commitSha)];

    public static List<RequestDictionaryModel> UsernameParam(string username) =>
        [ParamHelper.UrlSegment("username", username)];

    public static List<RequestDictionaryModel> InvitationIdParam(long id) =>
        [ParamHelper.UrlSegment("invitation_id", id)];

    public static List<RequestDictionaryModel> ReleaseIdParam(long id) =>
        [ParamHelper.UrlSegment("release_id", id)];

    public static List<RequestDictionaryModel> ReleaseTagParam(string tag) =>
        [ParamHelper.UrlSegment("release_tag", tag)];

    public static List<RequestDictionaryModel> SearchQueryParam(string query) =>
        [ParamHelper.Query(JsonFields.Q, query)];

    public static List<RequestDictionaryModel> StateParam(string state) =>
        [ParamHelper.Query(JsonFields.State, state)];
}
