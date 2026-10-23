using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
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
}
