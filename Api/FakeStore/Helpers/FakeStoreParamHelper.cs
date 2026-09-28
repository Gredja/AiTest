using Core.Models;

namespace Api.FakeStore.Helpers;

public static class FakeStoreParamHelper
{
    public static List<RequestDictionaryModel> IdParam(int id) =>
        [new() { Type = ParamType.UrlSegment, Key = "id", Value = id }];
}
