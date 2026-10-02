using Core.Config;
using Core.Helpers;
using TestAdapter;
using System.Net;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
[Description("Placeholder E2E test to verify Allure integration")]
public class SampleTests : GitHubE2ETestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("E2E-1 Sandbox repo is accessible via API")]
    public async Task SandboxRepo_IsAccessible()
    {
        var response = await Get<object>(GitHubEndpoints.ReposById,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }
}
