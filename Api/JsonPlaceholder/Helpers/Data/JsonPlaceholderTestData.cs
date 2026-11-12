using Core.Generators.JsonPlaceholder;
using Core.Models.JsonPlaceholder;

namespace Api.JsonPlaceholder.Helpers;

public static class JsonPlaceholderTestData
{
    public const string MalformedJson = "{\"title\":";
    public const string SampleTitle = "Title";
    public const string SampleBody = "Body";

    public static PostModelRequest TestPost =>
        PostGenerator.Create(1, "Test Post for contract", "Guarantee data");

    public static TodoModelRequest TestTodo =>
        TodoGenerator.Create(1, "Test Todo for contract");
}
