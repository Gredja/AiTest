using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Tests;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetTodosByUserIdTests : JsonPlaceholderRequestHelper
{
    [Test]
    [Category("HealthCheck")]
    [Description("8.1 Status code is 200")]
    public async Task GetTodosByUserId_ReturnsOk()
    {
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(JsonPlaceholderEndpoints.TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("8.2 Response body is not empty")]
    public async Task GetTodosByUserId_ReturnsNonEmptyList()
    {
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(JsonPlaceholderEndpoints.TestUserId));

        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("8.3 Each item has valid fields")]
    public async Task GetTodosByUserId_EachItemHasValidFields()
    {
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(JsonPlaceholderEndpoints.TestUserId));

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
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(JsonPlaceholderEndpoints.TestUserId));

        response.Data!.Should().OnlyContain(t => t.UserId == JsonPlaceholderEndpoints.TestUserId);
    }

    [Test]
    [Category("Negative")]
    [Description("8.5 Returns empty list for non-existent user")]
    public async Task GetTodosByUserId_NonExistentUser_ReturnsEmpty()
    {
        var allUsers = await Get<List<JsonPlaceholderUserModel>>(JsonPlaceholderEndpoints.Users, Method.Get);
        var maxUserId = allUsers.Data!.Max(u => u.Id);
        var nonExistentUserId = maxUserId + 1;

        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.TodosByUser, Method.Get,
            UserIdParam(nonExistentUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    private static List<RequestDictionaryModel> UserIdParam(int userId)
    {
        return new List<RequestDictionaryModel>
        {
            new() { Type = "Parameter", Key = "userId", Value = userId }
        };
    }
}
