using RestSharp;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.FakeStore.Helpers.FakeStoreParamHelper;

namespace Api.FakeStore.Users;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetUserByIdTests : RequestHelper
{
    private const int TestUserId = 1;
    private const int BoundaryUserId = 5;
    private const int LastUserId = 10;
    [Test]
    [Category("HealthCheck")]
    [Description("4.1 Get user by ID = 1 — status code 200")]
    public async Task GetUserById_ValidId_ReturnsOk()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("4.2 Response has all expected fields")]
    public async Task GetUserById_ValidId_HasAllExpectedFields()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(TestUserId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("4.3 `id` in response matches requested ID")]
    public async Task GetUserById_ValidId_IdMatchesRequested()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(TestUserId));

        response.Data!.Id.Should().Be(TestUserId);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
    [Category("Negative")]
    [Description("4.4 Get user by non-existent ID (maxId + 1) — status code 404")]
    public async Task GetUserById_NonExistentId_ReturnsNotFound()
    {
        var allUsers = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);
        var maxId = allUsers.Data!.Max(u => u.Id);
        var nonExistentId = maxId + 1;

        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for ID=0")]
    [Category("Negative")]
    [Description("4.5 Get user by ID = 0 — status code 404")]
    public async Task GetUserById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for negative IDs")]
    [Category("Negative")]
    [Description("4.6 Get user by negative ID (-1) — status code 404")]
    public async Task GetUserById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("4.7 Get user by ID = 5 returns valid user")]
    public async Task GetUserById_Id5_ReturnsValidUser()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(BoundaryUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(BoundaryUserId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("4.8 Get user by ID = 10 (last) returns valid user")]
    public async Task GetUserById_Id10_ReturnsValidUser()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(LastUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(LastUserId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("4.9 User by ID = 1 returns same user on repeated calls")]
    public async Task GetUserById_RepeatedCalls_ReturnSameUser()
    {
        var response1 = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(TestUserId));
        var response2 = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, IdParam(TestUserId));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Email.Should().Be(response2.Data!.Email);
        response1.Data!.Username.Should().Be(response2.Data!.Username);
    }
}
