using NUnit.Framework;
using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.GitHub.Helpers.GitHubParamHelper;

namespace Api.GitHub.Repos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetRepositoryTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("1.1 GET /repos/{owner}/{repo} returns 200 OK")]
    public async Task GetRepository_ReturnsOk()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<RepositoryModel>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("1.2 Response contains valid repository fields")]
    public async Task GetRepository_HasValidFields()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<RepositoryModel>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));

        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be(repo);
        response.Data.Owner.Should().NotBeNull();
        response.Data.Owner.Login.Should().Be(owner);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Content-Type is application/json")]
    public async Task GetRepository_ContentTypeIsJson()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<RepositoryModel>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Performance")]
    [Description("1.4 Response time < 5 seconds")]
    public async Task GetRepository_ResponseTimeIsAcceptable()
    {
        var (owner, repo) = ParseRepo();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<RepositoryModel>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(GitHubEndpoints.MaxResponseTimeMs);
    }

    [Test]
    [Category("Negative")]
    [Description("1.5 Non-existent repo returns 404")]
    public async Task GetRepository_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<RepositoryModel>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(GitHubEndpoints.TestUserId, GitHubEndpoints.NonExistentRepo));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
