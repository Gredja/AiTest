using Core.Helpers;
using Core.Models.Generic;

namespace Api.FakeStore.Helpers;

public static class FakeStoreParamHelper
{
    public static List<RequestDictionaryModel> CategoryParam(string category) =>
        [ParamHelper.UrlSegment("category", category)];
}
