using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using AllureAdapter;

namespace Api.GitHub.Repos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetReposTests : GitHubTestBase
{
    [Test]
    [Category("Negative")]
    [Description("1a.1 GET /repos returns 404 (no such route)")]
    public async Task GetRepos_NoRoute_ReturnsNotFound()
    {
        var response = await Get<object>(GitHubEndpoints.Repos);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
