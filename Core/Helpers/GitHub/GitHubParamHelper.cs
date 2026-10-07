using Core.Models.Generic;

namespace Core.Helpers.GitHub;

public static class GitHubParamHelper
{
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
            ParamHelper.Query("page", page),
            ParamHelper.Query("per_page", perPage)
        ];

    public static List<RequestDictionaryModel> UsernameParam(string username) =>
        [ParamHelper.UrlSegment("username", username)];

    public static List<RequestDictionaryModel> StateParam(string state) =>
        [ParamHelper.Query("state", state)];
}
