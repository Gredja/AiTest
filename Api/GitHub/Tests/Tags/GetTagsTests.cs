using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Tags;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetTagsTests : GitHubTestBase
{
    private static readonly Regex _shaHexPattern = new("^[0-9a-f]{40}$", RegexOptions.Compiled);
    private const string SpecialCharsSegment = "sp@ec!al";

    [Test]
    [Category("HealthCheck")]
    [Description("15.1 GET /tags returns 200 OK")]
    public async Task GetTags_ReturnsOk()
    {
        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("15.2 Response matches expected contract")]
    public async Task GetTags_ResponseMatchesContract()
    {
        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
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
    [Description("15.3 Every item: name non-empty, commit.sha is 40 hex chars")]
    public async Task GetTags_EveryItemHasValidFields()
    {
        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(
            tag => tag.Name.Length > 0 && _shaHexPattern.IsMatch(tag.Commit.Sha),
            "every tag needs a non-empty name and a 40-hex commit.sha");
    }

    [Test]
    [Category("Smoke")]
    [Description("15.4 Content-Type is application/json")]
    public async Task GetTags_ContentTypeIsJson()
    {
        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            TestRepoParam());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("15.5 Name values are unique")]
    public async Task GetTags_NamesAreUnique()
    {
        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Select(tag => tag.Name).Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Performance")]
    [Description("15.6 Response time < 5 seconds")]
    public async Task GetTags_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            TestRepoParam());
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("15.7 Repo without tags returns 200 with empty list (documented empty)")]
    public async Task GetTags_RepoWithoutTags_ReturnsOkWithEmptyList()
    {
        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull("empty list must be a valid body — OB §15");
    }

    [Test]
    [Category("Negative")]
    [Description("15.8 Non-existent repo returns 404")]
    public async Task GetTags_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15.9 Non-existent owner returns 404")]
    public async Task GetTags_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            RepoParam(GitHubEndpoints.NonExistentUser, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15.10 Invalid token returns 401")]
    public async Task GetTags_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoTags, InvalidAuthorization, TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("15.11 Owner with special chars returns 404")]
    public async Task GetTags_OwnerWithSpecialChars_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            RepoParam(SpecialCharsSegment, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("15.12 Repo with special chars returns 404")]
    public async Task GetTags_RepoWithSpecialChars_ReturnsNotFound()
    {
        var (owner, _) = ParseRepo();

        var response = await Get<List<TagModelResponse>>(GitHubEndpoints.RepoTags,
            RepoParam(owner, SpecialCharsSegment));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
