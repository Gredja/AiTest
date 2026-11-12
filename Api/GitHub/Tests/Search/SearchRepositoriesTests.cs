using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using static Core.Helpers.GitHub.GitHubParamHelper;
using static Core.Helpers.TestDataConstants;

namespace Api.GitHub.Search;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class SearchRepositoriesTests : GitHubTestBase
{
    private static readonly string RepositoryQuery = $"user:{TestConfig.GitHubTestUsername}";

    [Test]
    [Category("HealthCheck")]
    [Description("29.1 GET /search/repositories returns 200 OK")]
    public async Task SearchRepositories_ReturnsOk()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories, SearchQueryParam(RepositoryQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("29.2 Response matches expected contract")]
    public async Task SearchRepositories_ResponseMatchesContract()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories, SearchQueryParam(RepositoryQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Negative")]
    [Description("29.3 Missing q returns 422")]
    public async Task SearchRepositories_MissingQuery_Returns422()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Regression")]
    [Description("29.4 total_count is non-negative and items fit the count")]
    public async Task SearchRepositories_TotalCountIsConsistent()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories, SearchQueryParam(RepositoryQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.TotalCount.Should().BeGreaterThanOrEqualTo(response.Data.Items.Count);
        response.Data.Items.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("29.5 Each returned repository has name, owner and html_url")]
    public async Task SearchRepositories_ItemsHaveRepositoryFields()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories, SearchQueryParam(RepositoryQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Items.Should().OnlyContain(repository =>
            !string.IsNullOrWhiteSpace(repository.Name)
            && repository.Owner != null
            && !string.IsNullOrWhiteSpace(repository.HtmlUrl));
    }

    [Test]
    [Category("Smoke")]
    [Description("29.6 Content-Type is application/json")]
    public async Task SearchRepositories_ContentTypeIsJson()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories, SearchQueryParam(RepositoryQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("29.7 Garbage qualifier in q returns 422")]
    public async Task SearchRepositories_GarbageQualifier_Returns422()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories, SearchQueryParam(GarbageQualifierQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("29.8 Query longer than 256 chars returns 422")]
    public async Task SearchRepositories_OversizedQuery_Returns422()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories,
            SearchQueryParam(new string('a', GitHubEndpoints.SearchQueryMaxLength + 1)));

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("29.9 Logical operators without search terms return 422")]
    public async Task SearchRepositories_LogicalOperatorsOnly_Returns422()
    {
        var response = await Get<SearchModelResponse<RepositoryModelResponse>>(
            GitHubEndpoints.SearchRepositories, SearchQueryParam(LogicalOperatorsOnlyQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("29.10 Invalid token returns 401")]
    public async Task SearchRepositories_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.SearchRepositories, InvalidAuthorization, SearchQueryParam(RepositoryQuery));

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
