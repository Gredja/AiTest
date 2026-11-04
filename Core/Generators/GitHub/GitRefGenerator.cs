using Core.Models.GitHub;

namespace Core.Generators.GitHub;

public static class GitRefGenerator
{
    public static CreateGitRefModelRequest Create(string fullRef, string sha) =>
        new() { Ref = fullRef, Sha = sha };

    public static CreateGitRefModelRequest CreateRefOnly(string fullRef) =>
        new() { Ref = fullRef };
}
