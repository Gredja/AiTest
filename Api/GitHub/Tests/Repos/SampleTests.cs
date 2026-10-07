using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using AllureAdapter;

namespace Api.GitHub.Repos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class SampleTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("1.1.4 Sandbox repo is accessible via API")]
    public async Task SandboxRepo_IsAccessible()
    {
        var response = await Get<object>(GitHubEndpoints.ReposById,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }
}
