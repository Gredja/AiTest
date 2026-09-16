using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Todos;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetTodosByUserIdTests : JsonPlaceholderRequestHelper
{
    private const int TestUserId = 1;
    [Test]
    [Category("HealthCheck")]
    [Description("8.1 Status code is 200")]
    public async Task GetTodosByUserId_ReturnsOk()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("8.2 Response body is not empty")]
    public async Task GetTodosByUserId_ReturnsNonEmptyList()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(TestUserId));

        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("8.3 Each item has valid fields")]
    public async Task GetTodosByUserId_EachItemHasValidFields()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(TestUserId));

        foreach (var todo in response.Data!)
        {
            todo.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("8.4 All items belong to same user")]
    public async Task GetTodosByUserId_AllBelongToSameUser()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(TestUserId));

        response.Data!.Should().OnlyContain(t => t.UserId == TestUserId);
    }

    [Test]
    [Category("Negative")]
    [Description("8.5 Returns empty list for non-existent user")]
    public async Task GetTodosByUserId_NonExistentUser_ReturnsEmpty()
    {
        var allUsers = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users, Method.Get);
        var maxUserId = allUsers.Data!.Max(u => u.Id);
        var nonExistentUserId = maxUserId + 1;

        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(nonExistentUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("8.6 UserId=5 returns different subset than UserId=1")]
    public async Task GetTodosByUserId_DifferentUser_ReturnsDifferentSubset()
    {
        var response1 = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(TestUserId));
        var response5 = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(5));

        var ids1 = response1.Data!.Select(t => t.Id).ToList();
        var ids5 = response5.Data!.Select(t => t.Id).ToList();
        ids1.Should().NotBeEquivalentTo(ids5);
    }

    [Test]
    [Category("Negative")]
    [Description("8.7 UserId=0 returns empty list")]
    public async Task GetTodosByUserId_ZeroUserId_ReturnsEmpty()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }
}
