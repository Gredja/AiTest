using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;
using static Core.Helpers.TestDataConstants;

namespace Api.JsonPlaceholder.Users;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAllUsersTests : JsonPlaceholderRequestHelper
{
    private const int TestUserId = 1;
    private const string NonExistentUsername = "notexist";
    private const string UnknownQueryKey = "foo";
    private const string UnknownQueryValue = "bar";

    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAllUsers_ReturnsOk()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("1.2 Response matches expected contract")]
    public async Task GetAllUsers_ResponseMatchesContract()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Response body is not empty")]
    public async Task GetAllUsers_ReturnsNonEmptyList()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.4 Content-Type is application/json")]
    public async Task GetAllUsers_ContentTypeIsJson()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("1.5 Each item has valid required fields (via attributes)")]
    public async Task GetAllUsers_EachItemHasValidFields()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.ForEach(user => user.ShouldHaveValidFields());
    }

    [Test]
    [Category("Smoke")]
    [Description("1.6 Returns exactly 10 users")]
    public async Task GetAllUsers_ReturnsExpectedCount()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(TestConfig.ExpectedUserCount);
    }

    [Test]
    [Category("Regression")]
    [Description("1.7 All user IDs are unique")]
    public async Task GetAllUsers_AllIdsAreUnique()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        var ids = response.Data!.Select(user => user.Id);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("1.8 Each user has non-empty email")]
    public async Task GetAllUsers_EachUserHasValidEmail()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(user => !string.IsNullOrWhiteSpace(user.Email));
    }

    [Test]
    [Category("Performance")]
    [Description("1.9 Response time < 5 seconds")]
    public async Task GetAllUsers_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);
        stopwatch.Stop();

        AssertHelper.ShouldBeWithinResponseBudget(stopwatch.ElapsedMilliseconds);
    }

    [Test]
    [Category("Smoke")]
    [Description("14.1 Returns users matching existing username filter")]
    public async Task GetAllUsers_UsernameFilter_ReturnsMatchingUsers()
    {
        var allUsers = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);
        allUsers.ShouldHaveStatusCode(HttpStatusCode.OK);
        allUsers.Data.Should().NotBeEmpty();
        var existingUsername = allUsers.Data!.First().Username;

        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users,
            UsernameQueryParam(existingUsername));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data.Should().OnlyContain(user => user.Username == existingUsername);
    }

    [Test]
    [Category("Smoke")]
    [Description("14.2 Returns single user for existing id filter")]
    public async Task GetAllUsers_IdFilter_ReturnsMatchingUser()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users,
            IdQueryParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().ContainSingle(user => user.Id == TestUserId);
    }

    [Test]
    [Category("Regression")]
    [Description("14.3 Repeated calls return same data")]
    public async Task GetAllUsers_RepeatedCalls_ReturnSameData()
    {
        var firstResponse = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);
        firstResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        firstResponse.Data.Should().NotBeEmpty();

        var secondResponse = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);
        secondResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        secondResponse.Data.Should().BeEquivalentTo(firstResponse.Data);
    }

    [Test]
    [Category("Regression")]
    [Description("14.4 All ids are within expected range")]
    public async Task GetAllUsers_AllIdsWithinExpectedRange()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(user => user.Id >= 1 && user.Id <= TestConfig.ExpectedUserCount);
    }

    [Test]
    [Category("Smoke")]
    [Description("14.5 Trailing slash returns full list")]
    public async Task GetAllUsers_TrailingSlash_ReturnsFullList()
    {
        var response = await Get<List<UserModelResponse>>($"{JsonPlaceholderEndpoints.Users}/");

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(TestConfig.ExpectedUserCount);
    }

    [Test]
    [Category("Smoke")]
    [Description("14.6 Unknown query parameter is ignored")]
    public async Task GetAllUsers_UnknownQueryParam_ReturnsFullList()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users,
            [ParamHelper.Query(UnknownQueryKey, UnknownQueryValue)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(TestConfig.ExpectedUserCount);
    }

    [Test]
    [Category("Negative")]
    [Description("14.7 Returns empty list for non-existent id filter")]
    public async Task GetAllUsers_NonExistentId_ReturnsEmpty()
    {
        var allUsers = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);
        allUsers.ShouldHaveStatusCode(HttpStatusCode.OK);
        allUsers.Data.Should().NotBeEmpty();
        var maxUserId = allUsers.Data!.Max(user => user.Id);
        var nonExistentUserId = maxUserId + 1;

        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users,
            IdQueryParam(nonExistentUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("14.8 Returns empty list for id=0 filter")]
    public async Task GetAllUsers_ZeroId_ReturnsEmpty()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users,
            IdQueryParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("14.9 Returns empty list for id=-1 filter")]
    public async Task GetAllUsers_NegativeId_ReturnsEmpty()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users,
            IdQueryParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("14.10 Returns empty list for non-numeric id filter")]
    public async Task GetAllUsers_NonNumericId_ReturnsEmpty()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users,
            IdQueryParam(NonNumericValue));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("14.11 Returns empty list for non-existent username filter")]
    public async Task GetAllUsers_NonExistentUsername_ReturnsEmpty()
    {
        var response = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users,
            UsernameQueryParam(NonExistentUsername));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }
}
