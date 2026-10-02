using Core.Models;

namespace Api.JsonPlaceholder.Helpers;

public static class JsonPlaceholderParamHelper
{
    public static List<RequestDictionaryModel> IdParam(int id) =>
        [new() { Type = ParamType.UrlSegment, Key = "id", Value = id }];

    public static List<RequestDictionaryModel> UserIdParam(int userId) =>
        [new() { Type = ParamType.Parameter, Key = "userId", Value = userId }];

    public static List<RequestDictionaryModel> PostIdQueryParam(int postId) =>
        [new() { Type = ParamType.Parameter, Key = "postId", Value = postId }];

    public static List<RequestDictionaryModel> AlbumIdQueryParam(int albumId) =>
        [new() { Type = ParamType.Parameter, Key = "albumId", Value = albumId }];
}
