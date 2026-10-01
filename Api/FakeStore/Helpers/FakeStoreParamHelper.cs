using Core.Models;

namespace Api.FakeStore.Helpers;

public static class FakeStoreParamHelper
{
    public static List<RequestDictionaryModel> IdParam(int id) =>
        [new() { Type = ParamType.UrlSegment, Key = "id", Value = id }];

    public static List<RequestDictionaryModel> CategoryParam(string category) =>
        [new() { Type = ParamType.UrlSegment, Key = "category", Value = category }];
}
