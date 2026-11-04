using Core.Config;
using Core.Models.GitHub;

namespace Core.Generators.GitHub;

public static class PullRequestGenerator
{
    public static CreatePullRequestModelRequest Create(string title, string head, string baseBranch) =>
        new() { Title = title, Head = head, Base = baseBranch };

    public static CreatePullRequestModelRequest CreateWithTitle(string title) =>
        new() { Title = title };

    public static UpdatePullRequestModelRequest PatchTitle(string title) =>
        new() { Title = title };

    public static UpdatePullRequestModelRequest Close() =>
        new() { State = GitHubEndpoints.StateClosed };
}
