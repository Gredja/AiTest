using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Users;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAllUsersTests : JsonPlaceholderRequestHelper
{
    private const int ExpectedUserCount = 10;

    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAllUsers_ReturnsOk()
    {
        var response = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("1.2 Response matches expected contract")]
    public async Task GetAllUsers_ResponseMatchesContract()
    {
        var response = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Response body is not empty")]
    public async Task GetAllUsers_ReturnsNonEmptyList()
    {
        var response = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.4 Content-Type is application/json")]
    public async Task GetAllUsers_ContentTypeIsJson()
    {
        var response = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("1.5 Each item has valid required fields (via attributes)")]
    public async Task GetAllUsers_EachItemHasValidFields()
    {
        var response = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);

        foreach (var user in response.Data!)
        {
            user.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("1.6 Returns exactly 10 users")]
    public async Task GetAllUsers_ReturnsExpectedCount()
    {
        var response = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(ExpectedUserCount);
    }

    [Test]
    [Category("Regression")]
    [Description("1.7 All user IDs are unique")]
    public async Task GetAllUsers_AllIdsAreUnique()
    {
        var response = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);

        var ids = response.Data!.Select(u => u.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("1.8 Each user has non-empty email")]
    public async Task GetAllUsers_EachUserHasValidEmail()
    {
        var response = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);

        response.Data.Should().OnlyContain(u => !string.IsNullOrWhiteSpace(u.Email));
    }

    [Test]
    [Category("Performance")]
    [Description("1.9 Response time < 5 seconds")]
    public async Task GetAllUsers_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<JsonPlaceholderUser>>(JsonPlaceholderEndpoints.Users);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
