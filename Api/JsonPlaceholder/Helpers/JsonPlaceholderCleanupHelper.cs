using Core.Config;
using Core.Helpers;
using Core.Logging;
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
                    $"{CleanupWarning.Prefix}post {postId} not deleted: HTTP {(int)delete.StatusCode}");
            }
        }
        catch (HttpRequestException exception)
        {
            TestContext.Progress.WriteLine(
                $"{CleanupWarning.Prefix}failed to delete post {postId}: {exception.Message}");
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
                    $"{CleanupWarning.Prefix}todo {todoId} not deleted: HTTP {(int)delete.StatusCode}");
            }
        }
        catch (HttpRequestException exception)
        {
            TestContext.Progress.WriteLine(
                $"{CleanupWarning.Prefix}failed to delete todo {todoId}: {exception.Message}");
        }
    }
}
