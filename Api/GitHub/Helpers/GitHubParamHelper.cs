using Core.Models;

namespace Api.GitHub.Helpers;

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

    public static List<RequestDictionaryModel> PaginationParams(int page, int perPage) =>
        new()
        {
            new() { Type = ParamType.Parameter, Key = "page", Value = page },
            new() { Type = ParamType.Parameter, Key = "per_page", Value = perPage }
        };

    public static List<RequestDictionaryModel> UsernameParam(string username) =>
        new() { new() { Type = ParamType.UrlSegment, Key = "username", Value = username } };
}
