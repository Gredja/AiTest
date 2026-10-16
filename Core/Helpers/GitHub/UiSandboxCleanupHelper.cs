using Core.Config;

namespace Core.Helpers.GitHub;

public class UiSandboxCleanupHelper : GitHubCleanupHelper
{
    protected override string RepoFullName => TestConfig.UiTestRepo;

    protected override string? GitHubAuthToken => TestConfig.UiApiToken;
}
