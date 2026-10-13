using Core.Helpers;
using Core.Models.Generic;
using JsonFields = Core.Models.JsonPlaceholder.JsonFields;

namespace Api.JsonPlaceholder.Helpers;

public static class JsonPlaceholderParamHelper
{
    public static List<RequestDictionaryModel> UserIdParam(int userId) =>
        [ParamHelper.Query(JsonFields.UserId, userId)];

    public static List<RequestDictionaryModel> PostIdQueryParam(int postId) =>
        [ParamHelper.Query(JsonFields.PostId, postId)];

    public static List<RequestDictionaryModel> AlbumIdQueryParam(int albumId) =>
        [ParamHelper.Query(JsonFields.AlbumId, albumId)];
}