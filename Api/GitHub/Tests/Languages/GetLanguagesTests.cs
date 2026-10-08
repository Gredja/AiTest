using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Languages;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetLanguagesTests : GitHubTestBase
{
    private const string SpecialCharsSegment = "sp@ec!al";
    private const string InvalidPerPage = "abc";

    [Test]
    [Category("HealthCheck")]
    [Description("13.1 GET /languages returns 200 OK")]
    public async Task GetLanguages_ReturnsOk()
    {
        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("13.2 Response parses into language-to-bytes dictionary")]
    public async Task GetLanguages_ResponseMatchesContract()
    {
        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();

        var roundTrip = JsonSerializer.Deserialize<Dictionary<string, long>>(
            JsonSerializer.Serialize(response.Data!));
        roundTrip.Should().BeEquivalentTo(response.Data, "language dictionary must survive a JSON round-trip");
    }

    [Test]
    [Category("Smoke")]
    [Description("13.3 Content-Type is application/json")]
    public async Task GetLanguages_ContentTypeIsJson()
    {
        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            TestRepoParam());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("13.4 Repo with code returns at least one language")]
    public async Task GetLanguages_ReturnsNonEmptyDictionary()
    {
        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty("repo with code must report at least one language");
    }

    [Test]
    [Category("Regression")]
    [Description("13.5 All byte counts are positive")]
    public async Task GetLanguages_ByteCountsArePositive()
    {
        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(
            language => language.Value > 0,
            "every language byte count must be positive");
    }

    [Test]
    [Category("Regression")]
    [Description("13.6 All language keys are non-empty")]
    public async Task GetLanguages_KeysAreNonEmpty()
    {
        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(
            language => language.Key.Length > 0,
            "language names must be non-empty");
    }

    [Test]
    [Category("Smoke")]
    [Description("13.7 per_page=abc is silently ignored and returns 200")]
    public async Task GetLanguages_InvalidPerPage_ReturnsOk()
    {
        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            [.. TestRepoParam(), .. PerPageParam(InvalidPerPage)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Negative")]
    [Description("13.8 Non-existent repo returns 404")]
    public async Task GetLanguages_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("13.9 Non-existent owner returns 404")]
    public async Task GetLanguages_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            RepoParam(GitHubEndpoints.NonExistentUser, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("13.10 Invalid token returns 401")]
    public async Task GetLanguages_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoLanguages, InvalidAuthorization, TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("13.11 Owner with special chars returns 404")]
    public async Task GetLanguages_OwnerWithSpecialChars_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            RepoParam(SpecialCharsSegment, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("13.12 Repo with special chars returns 404")]
    public async Task GetLanguages_RepoWithSpecialChars_ReturnsNotFound()
    {
        var (owner, _) = ParseRepo();

        var response = await Get<Dictionary<string, long>>(GitHubEndpoints.RepoLanguages,
            RepoParam(owner, SpecialCharsSegment));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
