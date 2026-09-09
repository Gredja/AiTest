using Core.Config;

namespace Core.Helpers;

public class GitHubRequestHelper : RequestHelper
{
    public GitHubRequestHelper()
    {
        UseGitHub();
    }
}
