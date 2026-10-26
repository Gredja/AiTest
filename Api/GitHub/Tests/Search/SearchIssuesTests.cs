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
public class SearchIssuesTests : GitHubTestBase
{
    private const string IssueQuery = "repo:Gredja/AiTest type:issue";

    [Test]
    [Category("HealthCheck")]
    [Description("29a.1 GET /search/issues returns 200 OK")]
    public async Task SearchIssues_ReturnsOk()
    {
        var response = await Get<SearchModelResponse<IssueModelResponse>>(
            GitHubEndpoints.SearchIssues, SearchQueryParam(IssueQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("29a.2 Response matches expected contract")]
    public async Task SearchIssues_ResponseMatchesContract()
    {
        var response = await Get<SearchModelResponse<IssueModelResponse>>(
            GitHubEndpoints.SearchIssues, SearchQueryParam(IssueQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("29a.3 total_count is non-negative and items fit the count")]
    public async Task SearchIssues_TotalCountIsConsistent()
    {
        var response = await Get<SearchModelResponse<IssueModelResponse>>(
            GitHubEndpoints.SearchIssues, SearchQueryParam(IssueQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.TotalCount.Should().BeGreaterThanOrEqualTo(response.Data.Items.Count);
        response.Data.Items.Should().NotBeEmpty();
        response.Data.IsIncompleteResults.Should().BeFalse();
    }

    [Test]
    [Category("Regression")]
    [Description("29a.4 Each returned issue carries a repository_url and number")]
    public async Task SearchIssues_ItemsHaveIssueFields()
    {
        var response = await Get<SearchModelResponse<IssueModelResponse>>(
            GitHubEndpoints.SearchIssues, SearchQueryParam(IssueQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Items.Should().OnlyContain(issue =>
            issue.Number > 0 && !string.IsNullOrWhiteSpace(issue.Title));
    }

    [Test]
    [Category("Negative")]
    [Description("29a.5 Missing q returns 422")]
    public async Task SearchIssues_MissingQuery_Returns422()
    {
        var response = await Get<SearchModelResponse<IssueModelResponse>>(
            GitHubEndpoints.SearchIssues);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Smoke")]
    [Description("29a.6 Content-Type is application/json")]
    public async Task SearchIssues_ContentTypeIsJson()
    {
        var response = await Get<SearchModelResponse<IssueModelResponse>>(
            GitHubEndpoints.SearchIssues, SearchQueryParam(IssueQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }
}
