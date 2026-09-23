using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Users;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetUserTodosTests : JsonPlaceholderRequestHelper
{
    private const int TestUserId = 1;

    [Test]
    [Category("HealthCheck")]
    [Description("4.1 Status code is 200")]
    public async Task GetUserTodos_ReturnsOk()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.UsersTodos,
            PostIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("4.2 Response matches expected contract")]
    public async Task GetUserTodos_ResponseMatchesContract()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.UsersTodos,
            PostIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("4.3 Response body is not empty")]
    public async Task GetUserTodos_ReturnsNonEmptyList()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.UsersTodos,
            PostIdParam(TestUserId));

        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("4.4 Each item has valid fields")]
    public async Task GetUserTodos_EachItemHasValidFields()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.UsersTodos,
            PostIdParam(TestUserId));

        foreach (var todo in response.Data!)
        {
            todo.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("4.5 All todos belong to same user")]
    public async Task GetUserTodos_AllBelongToSameUser()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.UsersTodos,
            PostIdParam(TestUserId));

        response.Data!.Should().OnlyContain(t => t.UserId == TestUserId);
    }

    [Test]
    [Category("Negative")]
    [Description("4.6 Returns empty list for non-existent userId")]
    public async Task GetUserTodos_NonExistentUserId_ReturnsEmpty()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.UsersTodos,
            PostIdParam(999999));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("4.7 Returns empty list for userId=0")]
    public async Task GetUserTodos_ZeroUserId_ReturnsEmpty()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.UsersTodos,
            PostIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }
}
