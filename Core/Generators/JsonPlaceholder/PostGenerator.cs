using Core.Models.JsonPlaceholder;

namespace Core.Generators.JsonPlaceholder;

public static class PostGenerator
{
    public static PostModelRequest Create(int userId) =>
        new()
        {
            UserId = userId,
            Title = DataGenerator.RandomTitle("Post"),
            Body = DataGenerator.RandomBody("Body")
        };

    public static PostModelRequest Create(int userId, string title) =>
        new()
        {
            UserId = userId,
            Title = title,
            Body = DataGenerator.RandomBody("Body")
        };

    public static PostModelRequest Create(int userId, string title, string body) =>
        new()
        {
            UserId = userId,
            Title = title,
            Body = body
        };

    public static PostModelRequest Update(int userId) =>
        new()
        {
            UserId = userId,
            Title = DataGenerator.RandomTitle("Updated"),
            Body = DataGenerator.RandomBody("Body")
        };

    public static PostModelRequest PatchTitle() =>
        new() { Title = DataGenerator.RandomTitle("Patched") };

    public static PostModelRequest Empty() =>
        new();
}
