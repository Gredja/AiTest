using Core.Models.GitHub;

namespace Core.Generators.GitHub;

public static class CommentGenerator
{
    public static CreateCommentModelRequest CreateRandom(string bodyPrefix) =>
        new() { Body = DataGenerator.RandomBody(bodyPrefix) };

    public static CreateCommentModelRequest CreateWithBody(string body) =>
        new() { Body = body };
}
