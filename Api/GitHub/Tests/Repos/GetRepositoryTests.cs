using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Repos;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetRepositoryTests : GitHubTestBase
{
    private static readonly string _testUsername = TestConfig.GitHubTestUsername;
    private const string InvalidOwnerName = "!invalid!";
    private const string InvalidRepoName = "bad repo name";

    [Test]
    [Category("HealthCheck")]
    [Description("1.1 GET /repos/{owner}/{repo} returns 200 OK")]
    public async Task GetRepository_ReturnsOk()
    {
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("1.2 Response matches expected contract")]
    public async Task GetRepository_ResponseMatchesContract()
    {
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("1.3 Response contains valid repository fields")]
    public async Task GetRepository_HasValidFields()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            TestRepoParam());

        response.Data!.ShouldHaveValidFields();
        response.Data!.Name.Should().Be(repo);
        response.Data.Owner.Login.Should().Be(owner);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.4 Content-Type is application/json")]
    public async Task GetRepository_ContentTypeIsJson()
    {
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            TestRepoParam());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Performance")]
    [Description("1.5 Response time < 5 seconds")]
    public async Task GetRepository_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            TestRepoParam());
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Negative")]
    [Description("1.6 Non-existent repo returns 404")]
    public async Task GetRepository_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            RepoParam(_testUsername, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("1.7 Non-existent owner returns 404")]
    public async Task GetRepository_NonExistentOwner_ReturnsNotFound()
    {
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            RepoParam(GitHubEndpoints.NonExistentUser, ParseRepo().Repo));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("1.8 Repository name matches request")]
    public async Task GetRepository_NameMatchesRequest()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            TestRepoParam());

        response.Data!.Name.Should().Be(repo);
        response.Data.Owner.Login.Should().Be(owner);
    }

    [Test]
    [Category("Regression")]
    [Description("1.9 Repeated calls return same data")]
    public async Task GetRepository_RepeatedCalls_ReturnSameData()
    {
        var response1 = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            TestRepoParam());
        var response2 = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            TestRepoParam());

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Name.Should().Be(response2.Data!.Name);
    }

    [Test]
    [Category("Negative")]
    [Description("1.10 Owner with invalid characters returns 404")]
    public async Task GetRepository_InvalidOwnerCharacters_ReturnsNotFound()
    {
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            RepoParam(InvalidOwnerName, _testUsername));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("1.11 Repo name with spaces returns 404")]
    public async Task GetRepository_RepoNameWithSpaces_ReturnsNotFound()
    {
        var response = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById,
            RepoParam(_testUsername, InvalidRepoName));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
