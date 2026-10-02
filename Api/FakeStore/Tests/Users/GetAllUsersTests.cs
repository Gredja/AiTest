using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.FakeStore.Helpers.FakeStoreTestData;

namespace Api.FakeStore.Users;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetAllUsersTests : RequestHelper
{
    private const int ExpectedUserCount = 10;
    private int? _createdUserId;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var response = await Get<List<UserModelResponse>>(FakeStoreEndpoints.Users);

        if (response.Data!.Count == 0)
        {
            var create = await Post<UserModelRequest, UserModelResponse>(
                FakeStoreEndpoints.Users, TestUser);
            _createdUserId = create.Data!.Id;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdUserId.HasValue)
        {
            try
            {
                var delete = await Delete<object>($"{FakeStoreEndpoints.Users}/{_createdUserId}");

                if (!delete.IsSuccessful)
                {
                    TestContext.Progress.WriteLine(
                        $"Warning: user {_createdUserId} not deleted: HTTP {(int)delete.StatusCode}");
                }
            }
            catch (HttpRequestException exception)
            {
                TestContext.Progress.WriteLine(
                    $"Warning: failed to delete user {_createdUserId}: {exception.Message}");
            }
        }
    }

    [Test]
    [Category("HealthCheck")]
    [Description("3.1 Status code is 200")]
    public async Task GetAllUsers_ReturnsOk()
    {
        var response = await Get<List<UserModelResponse>>(FakeStoreEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("3.2 Response matches expected contract")]
    public async Task GetAllUsers_ResponseMatchesContract()
    {
        var response = await Get<List<UserModelResponse>>(FakeStoreEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("3.3 Response body is not empty")]
    public async Task GetAllUsers_ReturnsNonEmptyList()
    {
        var response = await Get<List<UserModelResponse>>(FakeStoreEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Smoke")]
    [Description("3.5 Content-Type is application/json")]
    public async Task GetAllUsers_ContentTypeIsJson()
    {
        var response = await Get<List<UserModelResponse>>(FakeStoreEndpoints.Users);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("3.6 Each item has valid required fields (via attributes)")]
    public async Task GetAllUsers_EachItemHasValidFields()
    {
        var response = await Get<List<UserModelResponse>>(FakeStoreEndpoints.Users);

        foreach (var user in response.Data!)
        {
            user.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Performance")]
    [Description("3.7 Response time < 5 seconds")]
    public async Task GetAllUsers_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<UserModelResponse>>(FakeStoreEndpoints.Users);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.8 Returns exactly 10 users")]
    public async Task GetAllUsers_ReturnsExpectedCount()
    {
        var response = await Get<List<UserModelResponse>>(FakeStoreEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(ExpectedUserCount);
    }

    [Test]
    [Category("Regression")]
    [Description("3.9 All user IDs are unique")]
    public async Task GetAllUsers_AllIdsAreUnique()
    {
        var response = await Get<List<UserModelResponse>>(FakeStoreEndpoints.Users);

        var ids = response.Data!.Select(user => user.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Ignore("FakeStoreAPI: GET /users returns 200 for query with invalid params — Bug: documentation/Bugs/FakeStore/FS-010-list-endpoints-invalid-params-return-200.md")]
    [Category("Negative")]
    [Description("3.10 GET /users with invalid query param returns 400")]
    public async Task GetAllUsers_InvalidQueryParam_ReturnsBadRequest()
    {
        var response = await Get<List<UserModelResponse>>($"{FakeStoreEndpoints.Users}?invalid=true");

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }
}
