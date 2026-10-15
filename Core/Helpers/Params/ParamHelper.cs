using Core.Models.Generic;

namespace Core.Helpers;

public static class ParamHelper
{
    public static List<RequestDictionaryModel> IdParam(int id) =>
        [UrlSegment(JsonFields.Id, id)];

    public static RequestDictionaryModel UrlSegment(string key, object value) =>
        new() { Type = ParamType.UrlSegment, Key = key, Value = value };

    public static RequestDictionaryModel Query(string key, object value) =>
        new() { Type = ParamType.Parameter, Key = key, Value = value };

    public static RequestDictionaryModel Header(string key, object value) =>
        new() { Type = ParamType.Header, Key = key, Value = value };
}
