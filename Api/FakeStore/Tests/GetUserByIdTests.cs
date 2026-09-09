using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.FakeStore.Tests;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetUserByIdTests : RequestHelper
{
    private static List<RequestDictionaryModel> UserIdParam(int id) =>
        new() { new() { Type = "UrlSegment", Key = "id", Value = id } };

    [Test]
    [Category("HealthCheck")]
    [Description("4.1 Get user by ID = 1 — status code 200")]
    public async Task GetUserById_ValidId_ReturnsOk()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, UserIdParam(FakeStoreEndpoints.TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("4.3 Response has all expected fields")]
    public async Task GetUserById_ValidId_HasAllExpectedFields()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, UserIdParam(FakeStoreEndpoints.TestUserId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("4.4 `id` in response matches requested ID")]
    public async Task GetUserById_ValidId_IdMatchesRequested()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, UserIdParam(FakeStoreEndpoints.TestUserId));

        response.Data!.Id.Should().Be(FakeStoreEndpoints.TestUserId);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
    [Category("Negative")]
    [Description("4.5 Get user by non-existent ID (maxId + 1) — status code 404")]
    public async Task GetUserById_NonExistentId_ReturnsNotFound()
    {
        var allUsers = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);
        var maxId = allUsers.Data!.Max(u => u.Id);
        var nonExistentId = maxId + 1;

        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, UserIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for ID=0")]
    [Category("Negative")]
    [Description("4.6 Get user by ID = 0 — status code 404")]
    public async Task GetUserById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, UserIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for negative IDs")]
    [Category("Negative")]
    [Description("4.7 Get user by negative ID (-1) — status code 404")]
    public async Task GetUserById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, UserIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
