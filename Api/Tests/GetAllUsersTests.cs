using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Config;
using Api.Helpers;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.Tests;

[TestFixture]
[AllureNUnit]
public class GetAllUsersTests : RequestHelper
{
    [Test]
    [Description("3.1 Status code is 200")]
    public async Task GetAllUsers_ReturnsOk()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Description("3.2 Response body is not empty")]
    public async Task GetAllUsers_ReturnsNonEmptyList()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Description("3.3 Content-Type is application/json")]
    public async Task GetAllUsers_ContentTypeIsJson()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Description("3.4 Each item has `id` (integer)")]
    public async Task GetAllUsers_EachItemHasId()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.Data!.ShouldAllHaveValidUserIds();
    }

    [Test]
    [Description("3.5 Each item has `email` (string, not empty)")]
    public async Task GetAllUsers_EachItemHasEmail()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.Data!.ShouldAllHaveValidEmail();
    }

    [Test]
    [Description("3.6 Each item has `username` (string, not empty)")]
    public async Task GetAllUsers_EachItemHasUsername()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.Data!.ShouldAllHaveValidUsername();
    }

    [Test]
    [Description("3.7 Each item has `name` (object with firstname, lastname)")]
    public async Task GetAllUsers_EachItemHasName()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.Data!.ShouldAllHaveValidName();
    }

    [Test]
    [Description("3.8 Each item has `phone` (string, not empty)")]
    public async Task GetAllUsers_EachItemHasPhone()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.Data!.ShouldAllHaveValidPhone();
    }

    [Test]
    [Description("3.9 Each item has `address` (object with city, street, zipcode)")]
    public async Task GetAllUsers_EachItemHasAddress()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.Data!.ShouldAllHaveValidAddress();
    }

    [Test]
    [Description("3.10 Response time < 5 seconds")]
    public async Task GetAllUsers_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(Endpoints.MaxResponseTimeMs);
    }

    [Test]
    [Description("3.11 Returns exactly 10 users")]
    public async Task GetAllUsers_ReturnsExpectedCount()
    {
        var response = await Get<List<UserModel>>(Endpoints.Users, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(Endpoints.ExpectedUserCount);
    }
}
