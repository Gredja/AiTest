using NUnit.Framework;
using RestSharp;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.FakeStore.Tests;

[TestFixture]
[AllureNUnit]
public class GetAllUsersTests : RequestHelper
{
    [Test]
    [Description("3.1 Status code is 200")]
    public async Task GetAllUsers_ReturnsOk()
    {
        var response = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Description("3.2 Response body is not empty")]
    public async Task GetAllUsers_ReturnsNonEmptyList()
    {
        var response = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Description("3.3 Content-Type is application/json")]
    public async Task GetAllUsers_ContentTypeIsJson()
    {
        var response = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);

        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Description("3.4 Each item has valid required fields (via attributes)")]
    public async Task GetAllUsers_EachItemHasValidFields()
    {
        var response = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);

        foreach (var user in response.Data!)
        {
            user.ShouldHaveValidFields();
        }
    }

    [Test]
    [Description("3.5 Response time < 5 seconds")]
    public async Task GetAllUsers_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(FakeStoreEndpoints.MaxResponseTimeMs);
    }

    [Test]
    [Description("3.6 Returns exactly 10 users")]
    public async Task GetAllUsers_ReturnsExpectedCount()
    {
        var response = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(FakeStoreEndpoints.ExpectedUserCount);
    }
}
