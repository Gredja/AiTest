using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Users;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetUserByIdTests : JsonPlaceholderRequestHelper
{
    private const int TestUserId = 1;
    private const int BoundaryUserId = 5;
    private const int LastUserId = 10;

    [Test]
    [Category("HealthCheck")]
    [Description("2.1 Status code is 200 for valid ID")]
    public async Task GetUserById_ReturnsOk()
    {
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("2.2 Response matches expected contract")]
    public async Task GetUserById_ResponseMatchesContract()
    {
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("2.3 Response body is not null")]
    public async Task GetUserById_ReturnsNonNull()
    {
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("2.4 Each field has valid attributes")]
    public async Task GetUserById_HasValidFields()
    {
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(TestUserId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.5 Response contains correct ID")]
    public async Task GetUserById_ReturnsCorrectId()
    {
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(TestUserId));

        response.Data!.Id.Should().Be(TestUserId);
    }

    [Test]
    [Category("Negative")]
    [Description("2.6 Returns 404 for non-existent ID")]
    public async Task GetUserById_NonExistentId_ReturnsNotFound()
    {
        var allUsers = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);
        var maxUserId = allUsers.Data!.Max(u => u.Id);
        var nonExistentId = maxUserId + 1;

        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.7 Returns 404 for ID 0")]
    public async Task GetUserById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.8 Returns 404 for negative ID")]
    public async Task GetUserById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("2.9 Get user by ID = 5 returns valid user")]
    public async Task GetUserById_BoundaryId_ReturnsValidUser()
    {
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(BoundaryUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(BoundaryUserId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.10 Get user by ID = 10 (last) returns valid user")]
    public async Task GetUserById_LastId_ReturnsValidUser()
    {
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(LastUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(LastUserId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.11 Repeated calls return same data")]
    public async Task GetUserById_RepeatedCalls_ReturnSameData()
    {
        var response1 = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(TestUserId));
        var response2 = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(TestUserId));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Name.Should().Be(response2.Data!.Name);
        response1.Data!.Email.Should().Be(response2.Data!.Email);
    }

    [Test]
    [Category("Performance")]
    [Description("2.12 Response time < 5 seconds")]
    public async Task GetUserById_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<JsonPlaceholderUser>(JsonPlaceholderEndpoints.UsersById,
            PostIdParam(TestUserId));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
