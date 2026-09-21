using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Todos;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAllTodosTests : JsonPlaceholderRequestHelper
{
    private const int ExpectedTodoCount = 200;
    private const int TestUserId = 1;
    private static readonly PostModelRequest _testTodo = new() { UserId = TestUserId, Title = "Test Todo for contract" };
    private int? _createdTodoId;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.Todos);

        if (response.Data!.Count == 0)
        {
            var create = await Post<PostModelRequest, TodoModelResponse>(JsonPlaceholderEndpoints.Todos, _testTodo);
            _createdTodoId = create.Data!.Id;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdTodoId.HasValue)
        {
            await Delete<object>($"{JsonPlaceholderEndpoints.Todos}/{_createdTodoId}");
        }
    }

    [Test]
    [Category("HealthCheck")]
    [Description("7.1 Status code is 200")]
    public async Task GetAllTodos_ReturnsOk()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.Todos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("7.2 Response matches expected contract")]
    public async Task GetAllTodos_ResponseMatchesContract()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.Todos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("7.3 Content-Type is application/json")]
    public async Task GetAllTodos_ContentTypeIsJson()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.Todos);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("7.4 Response body is not empty")]
    public async Task GetAllTodos_ReturnsNonEmptyList()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.Todos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("7.5 Each item has valid fields")]
    public async Task GetAllTodos_EachItemHasValidFields()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.Todos);

        foreach (var todo in response.Data!)
        {
            todo.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Performance")]
    [Description("7.6 Response time < 5 seconds")]
    public async Task GetAllTodos_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.Todos);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("7.7 Returns exactly 200 todos")]
    public async Task GetAllTodos_ReturnsExpectedCount()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.Todos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(ExpectedTodoCount);
    }

    [Test]
    [Category("Regression")]
    [Description("7.8 All todo IDs are unique")]
    public async Task GetAllTodos_AllIdsAreUnique()
    {
        var response = await Get<List<TodoModelResponse>>(JsonPlaceholderEndpoints.Todos);

        var ids = response.Data!.Select(t => t.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }
}
