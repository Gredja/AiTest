namespace Core.Config;

public static class GitHubEndpoints
{
    public static string BaseUrl => TestConfig.GitHubBaseUrl;
    public static string Token => TestConfig.GitHubToken;

    public const string NonExistentUser = "this-user-definitely-does-not-exist-12345";
    public const string NonExistentRepoName = "nonexistent";
    public const string NonExistentBranchName = "nonexistent-branch-12345";
    public const string BranchWithSpacesName = "branch with spaces";
    public const string DefaultBranch = "main";
    public const string MercyPreviewAccept = "application/vnd.github.mercy-preview+json";

    // max + 1 is racy: a concurrent E2E write in the same run can create that exact number
    public const int NonExistentIdOffset = 100;
    public const string StateOpen = "open";
    public const string StateClosed = "closed";
    public const string StateAll = "all";

    public const string Repos = "/repos";
    public const string Repositories = "/repositories";
    public const string ReposById = "/repos/{owner}/{repo}";

    public const string RepoIssues = "/repos/{owner}/{repo}/issues";
    public const string RepoIssueById = "/repos/{owner}/{repo}/issues/{issue_number}";
    public const string RepoIssueComments = "/repos/{owner}/{repo}/issues/{issue_number}/comments";
    public const string RepoIssueCommentById = "/repos/{owner}/{repo}/issues/comments/{comment_id}";

    public const string RepoPullRequests = "/repos/{owner}/{repo}/pulls";
    public const string RepoPullRequestById = "/repos/{owner}/{repo}/pulls/{pull_number}";
    public const string RepoPullRequestFiles = "/repos/{owner}/{repo}/pulls/{pull_number}/files";
    public const string RepoPullRequestCommits = "/repos/{owner}/{repo}/pulls/{pull_number}/commits";
    public const string RepoPullRequestMerge = "/repos/{owner}/{repo}/pulls/{pull_number}/merge";

    public const string RepoBranches = "/repos/{owner}/{repo}/branches";
    public const string RepoBranchByName = "/repos/{owner}/{repo}/branches/{branch}";

    public const string RepoContributors = "/repos/{owner}/{repo}/contributors";
    public const string RepoLanguages = "/repos/{owner}/{repo}/languages";
    public const string RepoTopics = "/repos/{owner}/{repo}/topics";
    public const string RepoTags = "/repos/{owner}/{repo}/tags";
    public const string RepoCommits = "/repos/{owner}/{repo}/commits";
    public const string RepoReleases = "/repos/{owner}/{repo}/releases";

    public const string RepoGitRefs = "/repos/{owner}/{repo}/git/refs";
    public const string RepoGitRefById = "/repos/{owner}/{repo}/git/refs/{ref}";
    public const string RepoGitRefByName = "/repos/{owner}/{repo}/git/ref/{ref}";
    public const string RepoGitCommits = "/repos/{owner}/{repo}/git/commits";
    public const string RepoGitCommitsById = "/repos/{owner}/{repo}/git/commits/{commit_sha}";
    public const string RepoGitBlobs = "/repos/{owner}/{repo}/git/blobs";
    public const string RepoGitTrees = "/repos/{owner}/{repo}/git/trees";

    public const string RepoCollaborators = "/repos/{owner}/{repo}/collaborators";
    public const string RepoCollaboratorPermission = "/repos/{owner}/{repo}/collaborators/{username}/permission";
    public const string RepoCollaboratorByUsername = "/repos/{owner}/{repo}/collaborators/{username}";
    public const string RepoInvitations = "/repos/{owner}/{repo}/invitations";
    public const string RepoInvitationById = "/repos/{owner}/{repo}/invitations/{invitation_id}";

    public const string Users = "/users";
    public const string UsersById = "/users/{username}";
    public const string UsersRepos = "/users/{username}/repos";
    public const string AuthenticatedUserRepos = "/user/repos";

    public const string RateLimit = "/rate_limit";
}
