using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderTestData;

namespace Api.JsonPlaceholder.Todos;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetTodosByUserIdTests : JsonPlaceholderRequestHelper
{
    private const int TestUserId = 1;
    private int? _createdTodoId;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(TestUserId));

        if (response.Data!.Count == 0)
        {
            var create = await Post<TodoModelRequest, TodoModelResponse>(JsonPlaceholderEndpoints.Todos, TestTodo);
            _createdTodoId = create.Data!.Id;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdTodoId.HasValue)
        {
            try
            {
                var delete = await Delete<object>($"{JsonPlaceholderEndpoints.Todos}/{_createdTodoId}");

                if (!delete.IsSuccessful)
                {
                    TestContext.Progress.WriteLine(
                        $"Warning: todo {_createdTodoId} not deleted: HTTP {(int)delete.StatusCode}");
                }
            }
            catch (HttpRequestException exception)
            {
                TestContext.Progress.WriteLine(
                    $"Warning: failed to delete todo {_createdTodoId}: {exception.Message}");
            }
        }
    }

    [Test]
    [Category("HealthCheck")]
    [Description("8.1 Status code is 200")]
    public async Task GetTodosByUserId_ReturnsOk()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("8.2 Response matches expected contract")]
    public async Task GetTodosByUserId_ResponseMatchesContract()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("8.3 Response body is not empty")]
    public async Task GetTodosByUserId_ReturnsNonEmptyList()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(TestUserId));

        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("8.4 Each item has valid fields")]
    public async Task GetTodosByUserId_EachItemHasValidFields()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(TestUserId));

        foreach (var todo in response.Data!)
        {
            todo.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("8.5 All items belong to same user")]
    public async Task GetTodosByUserId_AllBelongToSameUser()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(TestUserId));

        response.Data!.Should().OnlyContain(todo => todo.UserId == TestUserId);
    }

    [Test]
    [Category("Negative")]
    [Description("8.6 Returns empty list for non-existent user")]
    public async Task GetTodosByUserId_NonExistentUser_ReturnsEmpty()
    {
        var allUsers = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);
        var maxUserId = allUsers.Data!.Max(user => user.Id);
        var nonExistentUserId = maxUserId + 1;

        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(nonExistentUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("8.7 UserId=5 returns different subset than UserId=1")]
    public async Task GetTodosByUserId_DifferentUser_ReturnsDifferentSubset()
    {
        var response1 = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(TestUserId));
        var response5 = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(5));

        var ids1 = response1.Data!.Select(todo => todo.Id);
        var ids5 = response5.Data!.Select(todo => todo.Id);
        ids1.Should().NotBeEquivalentTo(ids5);
    }

    [Test]
    [Category("Negative")]
    [Description("8.8 UserId=0 returns empty list")]
    public async Task GetTodosByUserId_ZeroUserId_ReturnsEmpty()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser,
            UserIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }
}
