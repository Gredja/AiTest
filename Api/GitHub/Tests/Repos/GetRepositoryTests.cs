using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Repos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetRepositoryTests : GitHubTestBase
{
    private const string TestUserId = "Gredja";
    [Test]
    [Category("HealthCheck")]
    [Description("1.1 GET /repos/{owner}/{repo} returns 200 OK")]
    public async Task GetRepository_ReturnsOk()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("1.2 Response contains valid repository fields")]
    public async Task GetRepository_HasValidFields()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));

        response.Data!.ShouldHaveValidFields();
        response.Data!.Name.Should().Be(repo);
        response.Data.Owner.Login.Should().Be(owner);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Content-Type is application/json")]
    public async Task GetRepository_ContentTypeIsJson()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, Method.Get,
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
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Negative")]
    [Description("1.5 Non-existent repo returns 404")]
    public async Task GetRepository_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(TestUserId, GitHubEndpoints.NonExistentRepo));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("1.6 Non-existent owner returns 404")]
    public async Task GetRepository_NonExistentOwner_ReturnsNotFound()
    {
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(GitHubEndpoints.NonExistentUser, "AiTest"));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("1.7 Repository name matches request")]
    public async Task GetRepository_NameMatchesRequest()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));

        response.Data!.Name.Should().Be(repo);
        response.Data.Owner.Login.Should().Be(owner);
    }

    [Test]
    [Category("Regression")]
    [Description("1.8 Repeated calls return same data")]
    public async Task GetRepository_RepeatedCalls_ReturnSameData()
    {
        var (owner, repo) = ParseRepo();
        var response1 = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));
        var response2 = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, Method.Get,
            RepoParam(owner, repo));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Name.Should().Be(response2.Data!.Name);
    }
}
