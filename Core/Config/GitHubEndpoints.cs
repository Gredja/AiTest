namespace Core.Config;

public static class GitHubEndpoints
{
    public static string BaseUrl => TestConfig.GitHubBaseUrl;
    public static string Token => TestConfig.GitHubToken;
    public static int MaxResponseTimeMs => TestConfig.MaxResponseTimeMs;

    public static string TestRepo => TestConfig.GitHubTestRepo;
    public static string TestUserId => TestConfig.GitHubTestUserId;

    public const string NonExistentRepo = "this-repo-definitely-does-not-exist-12345";
    public const string NonExistentUser = "this-user-definitely-does-not-exist-12345";

    public const string Repos = "/repos";
    public const string ReposById = "/repos/{owner}/{repo}";

    public const string RepoIssues = "/repos/{owner}/{repo}/issues";
    public const string RepoIssueById = "/repos/{owner}/{repo}/issues/{issue_number}";
    public const string RepoIssueComments = "/repos/{owner}/{repo}/issues/{issue_number}/comments";

    public const string RepoPullRequests = "/repos/{owner}/{repo}/pulls";
    public const string RepoPullRequestById = "/repos/{owner}/{repo}/pulls/{pull_number}";
    public const string RepoPullRequestFiles = "/repos/{owner}/{repo}/pulls/{pull_number}/files";
    public const string RepoPullRequestCommits = "/repos/{owner}/{repo}/pulls/{pull_number}/commits";

    public const string RepoBranches = "/repos/{owner}/{repo}/branches";
    public const string RepoBranchByName = "/repos/{owner}/{repo}/branches/{branch}";

    public const string RepoContributors = "/repos/{owner}/{repo}/contributors";
    public const string RepoLanguages = "/repos/{owner}/{repo}/languages";
    public const string RepoTopics = "/repos/{owner}/{repo}/topics";
    public const string RepoTags = "/repos/{owner}/{repo}/tags";
    public const string RepoCommits = "/repos/{owner}/{repo}/commits";
    public const string RepoReleases = "/repos/{owner}/{repo}/releases";

    public const string Users = "/users";
    public const string UsersById = "/users/{username}";
    public const string UsersRepos = "/users/{username}/repos";
    public const string AuthenticatedUserRepos = "/user/repos";

    public const string RateLimit = "/rate_limit";
}
