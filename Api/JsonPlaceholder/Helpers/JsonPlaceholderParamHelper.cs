using Core.Helpers;
using Core.Models.Generic;
using GenericJsonFields = Core.Models.Generic.JsonFields;
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

    public static List<RequestDictionaryModel> IdQueryParam(object id) =>
        [ParamHelper.Query(GenericJsonFields.Id, id)];

    public static List<RequestDictionaryModel> UsernameQueryParam(string username) =>
        [ParamHelper.Query(JsonFields.Username, username)];
}