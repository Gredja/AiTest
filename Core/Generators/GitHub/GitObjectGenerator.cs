using Core.Config;
using Core.Models.GitHub;

namespace Core.Generators.GitHub;

public static class GitObjectGenerator
{
    public static CreateGitBlobModelRequest CreateBlob(string content) =>
        new() { Content = content };

    public static CreateGitTreeModelRequest CreateTree(string baseTreeSha, string entryPath, string entrySha) =>
        new()
        {
            BaseTree = baseTreeSha,
            Tree =
            [
                new GitTreeEntry
                {
                    Path = entryPath,
                    Mode = GitHubEndpoints.FileMode644,
                    Type = GitHubEndpoints.ObjectTypeBlob,
                    Sha = entrySha
                }
            ]
        };

    public static CreateGitCommitModelRequest CreateCommit(string message, string treeSha, List<string> parents) =>
        new() { Message = message, Tree = treeSha, Parents = parents };
}
