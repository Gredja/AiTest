using Core.Config;

namespace Core.Helpers.GitHub;

// Cleanup against the UI sandbox repo (gredjapl/ui-sandbox) with GITHUB_UI_PAT —
// separate from GitHubCleanupHelper which targets Gredja/AiTest via GITHUB_PAT.
public class UiSandboxCleanupHelper : GitHubCleanupHelper
{
    protected override string RepoFullName => TestConfig.UiTestRepo;

    protected override string? GitHubAuthToken => TestConfig.UiApiToken;
}
