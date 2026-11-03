using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Search;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class SearchUsersTests : GitHubTestBase
{
    private const string UserQuery = "Gredja";
    private const string GarbageQualifierQuery = "repo:";
    private const string LogicalOperatorsOnlyQuery = "AND";

    [Test]
    [Category("HealthCheck")]
    [Description("29b.1 GET /search/users returns 200 OK")]
    public async Task SearchUsers_ReturnsOk()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers, SearchQueryParam(UserQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("29b.2 Response matches expected contract")]
    public async Task SearchUsers_ResponseMatchesContract()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers, SearchQueryParam(UserQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("29b.3 total_count is non-negative and items are present")]
    public async Task SearchUsers_TotalCountIsConsistent()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers, SearchQueryParam(UserQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.TotalCount.Should().BeGreaterThanOrEqualTo(response.Data.Items.Count);
        response.Data.Items.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("29b.4 Each returned user has login and positive id")]
    public async Task SearchUsers_ItemsHaveUserFields()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers, SearchQueryParam(UserQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Items.Should().OnlyContain(user =>
            !string.IsNullOrWhiteSpace(user.Login) && user.Id > 0);
    }

    [Test]
    [Category("Negative")]
    [Description("29b.5 Missing q returns 422")]
    public async Task SearchUsers_MissingQuery_Returns422()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Smoke")]
    [Description("29b.6 Content-Type is application/json")]
    public async Task SearchUsers_ContentTypeIsJson()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers, SearchQueryParam(UserQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("29b.7 Garbage qualifier in q returns 422")]
    public async Task SearchUsers_GarbageQualifier_Returns422()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers, SearchQueryParam(GarbageQualifierQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("29b.8 Query longer than 256 chars returns 422")]
    public async Task SearchUsers_OversizedQuery_Returns422()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers,
            SearchQueryParam(new string('a', GitHubEndpoints.SearchQueryMaxLength + 1)));

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("29b.9 Logical operators without search terms return 422")]
    public async Task SearchUsers_LogicalOperatorsOnly_Returns422()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers, SearchQueryParam(LogicalOperatorsOnlyQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("29b.10 Invalid token returns 401")]
    public async Task SearchUsers_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.SearchUsers, InvalidAuthorization, SearchQueryParam(UserQuery));

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
