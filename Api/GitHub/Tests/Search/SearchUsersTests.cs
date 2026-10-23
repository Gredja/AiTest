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
public class SearchUsersTests : GitHubTestBase
{
    private const string UserQuery = "Gredja";

    [Test]
    [Category("HealthCheck")]
    [Description("29b.1 GET /search/users returns 200 OK")]
    public async Task SearchUsers_ReturnsOk()
    {
        var response = await Get<SearchModelResponse<UserModelResponse>>(
            GitHubEndpoints.SearchUsers, SearchQueryParam(UserQuery));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }
}
