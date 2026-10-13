using Core.Models.Generic;

namespace Core.Helpers;

public static class ParamHelper
{
    // Wire key "id" lives in Generic.JsonFields (internal) — this public alias is the
    // only cross-service exposure; Api/E2E cannot see the internal constant.
    public const string IdKey = JsonFields.Id;

    public static List<RequestDictionaryModel> IdParam(int id) =>
        [UrlSegment(IdKey, id)];

    public static RequestDictionaryModel UrlSegment(string key, object value) =>
        new() { Type = ParamType.UrlSegment, Key = key, Value = value };

    public static RequestDictionaryModel Query(string key, object value) =>
        new() { Type = ParamType.Parameter, Key = key, Value = value };

    public static RequestDictionaryModel Header(string key, object value) =>
        new() { Type = ParamType.Header, Key = key, Value = value };
}
