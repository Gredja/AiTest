using Core.Helpers;
using Core.Models;

namespace Api.JsonPlaceholder.Helpers;

public static class JsonPlaceholderParamHelper
{
    public static List<RequestDictionaryModel> UserIdParam(int userId) =>
        [ParamHelper.Query("userId", userId)];

    public static List<RequestDictionaryModel> PostIdQueryParam(int postId) =>
        [ParamHelper.Query("postId", postId)];

    public static List<RequestDictionaryModel> AlbumIdQueryParam(int albumId) =>
        [ParamHelper.Query("albumId", albumId)];
}
