using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Releases;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetReleasesTests : GitHubTestBase
{
    private const string InvalidPerPage = "abc";
    private const string SpecialCharsSegment = "sp@ec!al";

    [Test]
    [Category("HealthCheck")]
    [Description("15b.1 GET /releases returns 200 OK")]
    public async Task GetReleases_ReturnsOk()
    {
        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("15b.2 Response matches expected contract when items exist")]
    public async Task GetReleases_ResponseMatchesContract()
    {
        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();

        if (response.Data.Count > 0)
        {
            response.Data.First().ShouldHaveValidContract();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("15b.3 Every item has valid fields (tag_name non-empty, id > 0)")]
    public async Task GetReleases_EveryItemHasValidFields()
    {
        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(
            release => release.TagName.Length > 0 && release.Id > 0,
            "every release needs a non-empty tag_name and positive id");
    }

    [Test]
    [Category("Smoke")]
    [Description("15b.4 Content-Type is application/json")]
    public async Task GetReleases_ContentTypeIsJson()
    {
        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            TestRepoParam());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("15b.5 tag_name values are unique")]
    public async Task GetReleases_TagNamesAreUnique()
    {
        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Select(release => release.TagName).Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Performance")]
    [Description("15b.6 Response time < 5 seconds")]
    public async Task GetReleases_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            TestRepoParam());
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("15b.7 Repo without releases returns 200 with empty list (documented empty)")]
    public async Task GetReleases_RepoWithoutReleases_ReturnsOkWithEmptyList()
    {
        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull("empty list must be a valid body — OB §15b");
    }

    [Test]
    [Category("Smoke")]
    [Description("15b.8 per_page=abc is silently ignored and returns 200")]
    public async Task GetReleases_InvalidPerPage_ReturnsOk()
    {
        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            [.. TestRepoParam(), .. PerPageParam(InvalidPerPage)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Negative")]
    [Description("15b.9 Non-existent repo returns 404")]
    public async Task GetReleases_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15b.10 Non-existent owner returns 404")]
    public async Task GetReleases_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            RepoParam(GitHubEndpoints.NonExistentUser, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15b.11 Invalid token returns 401")]
    public async Task GetReleases_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoReleases, InvalidAuthorization, TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("15b.12 Owner with special chars returns 404")]
    public async Task GetReleases_OwnerWithSpecialChars_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            RepoParam(SpecialCharsSegment, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15b.13 Repo with special chars returns 404")]
    public async Task GetReleases_RepoWithSpecialChars_ReturnsNotFound()
    {
        var (owner, _) = ParseRepo();

        var response = await Get<List<ReleaseModelResponse>>(GitHubEndpoints.RepoReleases,
            RepoParam(owner, SpecialCharsSegment));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
