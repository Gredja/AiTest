using Core.Models;

namespace Api.JsonPlaceholder.Helpers;

public static class JsonPlaceholderParamHelper
{
    public static List<RequestDictionaryModel> PostIdParam(int id) =>
        new() { new() { Type = ParamType.UrlSegment, Key = "id", Value = id } };

    public static List<RequestDictionaryModel> UserIdParam(int userId) =>
        new() { new() { Type = ParamType.Parameter, Key = "userId", Value = userId } };
}
