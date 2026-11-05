using Core.Config;
using Core.Helpers;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderTestData;
using static Core.Helpers.ParamHelper;

namespace Api.JsonPlaceholder.Helpers;

public class JsonPlaceholderCleanupHelper : JsonPlaceholderRequestHelper
{
    public async Task CleanupPostAsync(int? postId)
    {
        if (!postId.HasValue)
        {
            return;
        }

        try
        {
            var delete = await Delete<object>(JsonPlaceholderEndpoints.PostsById, IdParam(postId.Value));

            if (!delete.IsSuccessful)
            {
                TestContext.Progress.WriteLine(
                    string.Format(PostNotDeletedWarningFormat, postId, (int)delete.StatusCode));
            }
        }
        catch (HttpRequestException exception)
        {
            TestContext.Progress.WriteLine(
                string.Format(PostDeleteFailedWarningFormat, postId, exception.Message));
        }
    }

    public async Task CleanupTodoAsync(int? todoId)
    {
        if (!todoId.HasValue)
        {
            return;
        }

        try
        {
            var delete = await Delete<object>(JsonPlaceholderEndpoints.TodosById, IdParam(todoId.Value));

            if (!delete.IsSuccessful)
            {
                TestContext.Progress.WriteLine(
                    string.Format(TodoNotDeletedWarningFormat, todoId, (int)delete.StatusCode));
            }
        }
        catch (HttpRequestException exception)
        {
            TestContext.Progress.WriteLine(
                string.Format(TodoDeleteFailedWarningFormat, todoId, exception.Message));
        }
    }
}
