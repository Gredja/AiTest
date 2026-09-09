using NUnit.Framework;
using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Tests;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAllTodosTests : JsonPlaceholderRequestHelper
{
    [Test]
    [Category("HealthCheck")]
    [Description("7.1 Status code is 200")]
    public async Task GetAllTodos_ReturnsOk()
    {
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.Todos, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("7.2 Content-Type is application/json")]
    public async Task GetAllTodos_ContentTypeIsJson()
    {
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.Todos, Method.Get);

        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Category("Smoke")]
    [Description("7.3 Response body is not empty")]
    public async Task GetAllTodos_ReturnsNonEmptyList()
    {
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.Todos, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("7.4 Each item has valid fields")]
    public async Task GetAllTodos_EachItemHasValidFields()
    {
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.Todos, Method.Get);

        foreach (var todo in response.Data!)
        {
            todo.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Performance")]
    [Description("7.5 Response time < 5 seconds")]
    public async Task GetAllTodos_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.Todos, Method.Get);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(JsonPlaceholderEndpoints.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("7.6 Returns exactly 200 todos")]
    public async Task GetAllTodos_ReturnsExpectedCount()
    {
        var response = await Get<List<TodoModel>>(JsonPlaceholderEndpoints.Todos, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(JsonPlaceholderEndpoints.ExpectedTodoCount);
    }
}
