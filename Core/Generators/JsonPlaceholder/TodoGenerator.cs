using Core.Models.JsonPlaceholder;

namespace Core.Generators.JsonPlaceholder;

public static class TodoGenerator
{
    public static TodoModelRequest Create(int userId, string title) =>
        new() { UserId = userId, Title = title };
}
