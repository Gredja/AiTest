using Core.Models.JsonPlaceholder;

namespace Api.JsonPlaceholder.Helpers;

public static class JsonPlaceholderTestData
{
    public const string TitleKey = "title";
    public const string BodyKey = "body";

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
