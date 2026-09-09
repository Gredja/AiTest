using Core.Config;
using Core.Helpers;

namespace Api.GitHub;

public abstract class GitHubTestBase : GitHubRequestHelper
{
    protected static (string Owner, string Repo) ParseRepo() => ParseRepo(GitHubEndpoints.TestRepo);

    private static (string Owner, string Repo) ParseRepo(string fullName)
    {
        var parts = fullName.Split('/');
        return (parts[0], parts[1]);
    }
}
