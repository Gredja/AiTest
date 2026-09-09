using Core.Models;

namespace Api.Helpers;

public static class FakeStoreParamHelper
{
    public static List<RequestDictionaryModel> ProductIdParam(int id) =>
        new() { new() { Type = ParamType.UrlSegment, Key = "id", Value = id } };

    public static List<RequestDictionaryModel> UserIdParam(int id) =>
        new() { new() { Type = ParamType.UrlSegment, Key = "id", Value = id } };
}
