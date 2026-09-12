using Core.Models;

namespace Core.Helpers.GitHub;

public static class GitHubParamHelper
{
    public static List<RequestDictionaryModel> RepoParam(string owner, string repo) =>
        new()
        {
            new() { Type = ParamType.UrlSegment, Key = "owner", Value = owner },
            new() { Type = ParamType.UrlSegment, Key = "repo", Value = repo }
        };

    public static List<RequestDictionaryModel> IssueNumberParam(int number) =>
        new() { new() { Type = ParamType.UrlSegment, Key = "issue_number", Value = number } };

    public static List<RequestDictionaryModel> PullNumberParam(int number) =>
        new() { new() { Type = ParamType.UrlSegment, Key = "pull_number", Value = number } };

    public static List<RequestDictionaryModel> BranchNameParam(string branch) =>
        new() { new() { Type = ParamType.UrlSegment, Key = "branch", Value = branch } };

    public static List<RequestDictionaryModel> PaginationParams(int page, int perPage) =>
        new()
        {
            new() { Type = ParamType.Parameter, Key = "page", Value = page },
            new() { Type = ParamType.Parameter, Key = "per_page", Value = perPage }
        };

    public static List<RequestDictionaryModel> UsernameParam(string username) =>
        new() { new() { Type = ParamType.UrlSegment, Key = "username", Value = username } };

    public static List<RequestDictionaryModel> StateParam(string state) =>
        new() { new() { Type = ParamType.Parameter, Key = "state", Value = state } };
}
