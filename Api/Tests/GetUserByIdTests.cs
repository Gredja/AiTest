using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.Tests;

[TestFixture]
[AllureNUnit]
[AllureEpic("API")]
[AllureFeature("Users")]
[AllureStory("Get User By ID")]
public class GetUserByIdTests : RequestHelper
{
    private static List<RequestDictionaryModel> UserIdParam(int id) =>
        new() { new() { Type = "UrlSegment", Key = "id", Value = id } };

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("4.1 Get user by ID = 1 — status code 200")]
    public async Task GetUserById_ValidId_ReturnsOk()
    {
        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(Endpoints.TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.3 Response has all expected fields")]
    public async Task GetUserById_ValidId_HasAllExpectedFields()
    {
        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(Endpoints.TestUserId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.4 `id` in response matches requested ID")]
    public async Task GetUserById_ValidId_IdMatchesRequested()
    {
        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(Endpoints.TestUserId));

        response.Data!.Id.Should().Be(Endpoints.TestUserId);
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.5 `email` is string, not empty")]
    public async Task GetUserById_ValidId_HasEmail()
    {
        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(Endpoints.TestUserId));

        response.Data!.Email.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.6 `username` is string, not empty")]
    public async Task GetUserById_ValidId_HasUsername()
    {
        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(Endpoints.TestUserId));

        response.Data!.Username.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.7 `name.firstname` and `name.lastname` are strings, not empty")]
    public async Task GetUserById_ValidId_HasName()
    {
        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(Endpoints.TestUserId));

        response.Data!.Name.Should().NotBeNull();
        response.Data.Name.Firstname.Should().NotBeNullOrWhiteSpace();
        response.Data.Name.Lastname.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.8 `phone` is string, not empty")]
    public async Task GetUserById_ValidId_HasPhone()
    {
        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(Endpoints.TestUserId));

        response.Data!.Phone.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Negative")]
    [Category("Slow")]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
    [Description("4.9 Get user by non-existent ID (maxId + 1) — status code 404")]
    public async Task GetUserById_NonExistentId_ReturnsNotFound()
    {
        var allUsers = await Get<List<UserModel>>(Endpoints.Users, Method.Get);
        var maxId = allUsers.Data!.Max(u => u.Id);
        var nonExistentId = maxId + 1;

        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("EdgeCase")]
    [Category("Slow")]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for ID=0")]
    [Description("4.10 Get user by ID = 0 — status code 404")]
    public async Task GetUserById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("EdgeCase")]
    [Category("Slow")]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for negative IDs")]
    [Description("4.11 Get user by negative ID (-1) — status code 404")]
    public async Task GetUserById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<UserModel>(Endpoints.UsersById, Method.Get, UserIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
