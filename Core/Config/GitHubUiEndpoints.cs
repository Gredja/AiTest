namespace Core.Config;

public static class GitHubUiEndpoints
{
    // Browser routes for the configured test repo (Ui.TestRepo) — UI page-objects build
    // URLs from here, never inline (Rules/config.md → Endpoints)
    public static string RepoPath => $"/{TestConfig.UiTestRepo}";
    public static string IssuesPath => $"{RepoPath}/issues";
    public static string NewIssuePath => $"{IssuesPath}/new";
    public static string PullsPath => $"{RepoPath}/pulls";
    public static string BranchesPath => $"{RepoPath}/branches";
    public static string CommitsPath => $"{RepoPath}/commits";

    public const string LoginPath = "/login";
    public const string IssuesFragment = "/issues";
    public const string PullFragment = "/pull/";

    public static string IssuePath(int issueNumber) => $"{IssuesPath}/{issueNumber}";

    public static string ComparePath(string baseBranch, string headBranch) =>
        $"{RepoPath}/compare/{baseBranch}...{headBranch}";

    public static string PullPath(int pullNumber) => $"{RepoPath}/pull/{pullNumber}";

    public static string PullFilesPath(int pullNumber) => $"{PullPath(pullNumber)}/files";

    public static string TreePath(string branchName) => $"{RepoPath}/tree/{branchName}";

    public static string TreeFolderPath(string branchName, string folderName) =>
        $"{TreePath(branchName)}/{folderName}";

    public static string BlobPath(string branchName, string fileName) =>
        $"{RepoPath}/blob/{branchName}/{fileName}";
}
