using NUnit.Framework;
using RestSharp;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using FluentAssertions;
using TestAdapter;
using System.Net;
using static Core.Helpers.GitHub.GitHubParamHelper;

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
        var (owner, repo) = ParseRepo();
        var response = await Get<object>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }
}
