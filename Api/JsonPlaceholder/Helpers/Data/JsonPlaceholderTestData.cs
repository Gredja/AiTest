using Core.Models.JsonPlaceholder;

namespace Api.JsonPlaceholder.Helpers;

public static class JsonPlaceholderTestData
{
    public const string MalformedJson = "{\"title\":";
    public const string SampleTitle = "Title";
    public const string SampleBody = "Body";
    public const string PostNotDeletedWarningFormat = "Warning: post {0} not deleted: HTTP {1}";
    public const string PostDeleteFailedWarningFormat = "Warning: failed to delete post {0}: {1}";
    public const string TodoNotDeletedWarningFormat = "Warning: todo {0} not deleted: HTTP {1}";
    public const string TodoDeleteFailedWarningFormat = "Warning: failed to delete todo {0}: {1}";

    public static readonly PostModelRequest TestPost = new()
    {
        UserId = 1,
        Title = "Test Post for contract",
        Body = "Guarantee data"
    };

    public static readonly TodoModelRequest TestTodo = new()
    {
        UserId = 1,
        Title = "Test Todo for contract"
    };
}
