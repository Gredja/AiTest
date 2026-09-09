namespace Core.Config;

public static class GitHubEndpoints
{
    public static string BaseUrl => TestConfig.GitHubBaseUrl;
    public static string Token => TestConfig.GitHubToken;
    public static int MaxResponseTimeMs => TestConfig.MaxResponseTimeMs;

    public static string TestRepo => TestConfig.GitHubTestRepo;
    public static string TestUserId => TestConfig.GitHubTestUserId;

    public const string Repos = "/repos";
    public const string ReposById = "/repos/{owner}/{repo}";

    public const string RepoIssues = "/repos/{owner}/{repo}/issues";
    public const string RepoIssueById = "/repos/{owner}/{repo}/issues/{issue_number}";

    public const string RepoPullRequests = "/repos/{owner}/{repo}/pulls";
    public const string RepoBranches = "/repos/{owner}/{repo}/branches";

    public const string Users = "/users";
    public const string UsersById = "/users/{username}";

    public const string RateLimit = "/rate_limit";
}
