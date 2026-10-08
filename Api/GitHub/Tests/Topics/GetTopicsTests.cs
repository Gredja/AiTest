using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Topics;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetTopicsTests : GitHubTestBase
{
    private static readonly Regex _topicNamePattern = new("^[a-z0-9-]+$", RegexOptions.Compiled);
    private const string SpecialCharsSegment = "sp@ec!al";

    [Test]
    [Category("HealthCheck")]
    [Description("14.1 GET /topics with mercy-preview Accept returns 200 OK")]
    public async Task GetTopics_ReturnsOk()
    {
        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            MercyPreviewRepoParams());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("14.2 Response matches expected contract")]
    public async Task GetTopics_ResponseMatchesContract()
    {
        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            MercyPreviewRepoParams());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("14.3 Names match lowercase alphanumeric-with-hyphens pattern")]
    public async Task GetTopics_NamesMatchPattern()
    {
        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            MercyPreviewRepoParams());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Names.Should().OnlyContain(
            name => _topicNamePattern.IsMatch(name),
            "topic names must be lowercase alphanumeric with hyphens");
    }

    [Test]
    [Category("Regression")]
    [Description("14.4 Name values are unique")]
    public async Task GetTopics_NamesAreUnique()
    {
        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            MercyPreviewRepoParams());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Names.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Smoke")]
    [Description("14.5 Content-Type is application/json")]
    public async Task GetTopics_ContentTypeIsJson()
    {
        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            MercyPreviewRepoParams());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("14.6 Repo without topics returns 200 with names array (documented empty)")]
    public async Task GetTopics_RepoWithoutTopics_ReturnsOkWithNames()
    {
        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            MercyPreviewRepoParams());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Names.Should().NotBeNull("names array must be present even when empty — OB §14");
    }

    [Test]
    [Category("Negative")]
    [Description("14.7 Non-existent repo returns 404")]
    public async Task GetTopics_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. MercyPreviewParam()]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("14.8 Non-existent owner returns 404")]
    public async Task GetTopics_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, repo), .. MercyPreviewParam()]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("14.9 Invalid token returns 401")]
    public async Task GetTopics_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoTopics, InvalidAuthorization, MercyPreviewRepoParams());

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("14.10 Owner with special chars returns 404")]
    public async Task GetTopics_OwnerWithSpecialChars_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            [.. RepoParam(SpecialCharsSegment, repo), .. MercyPreviewParam()]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("14.11 Repo with special chars returns 404")]
    public async Task GetTopics_RepoWithSpecialChars_ReturnsNotFound()
    {
        var (owner, _) = ParseRepo();

        var response = await Get<TopicsModelResponse>(GitHubEndpoints.RepoTopics,
            [.. RepoParam(owner, SpecialCharsSegment), .. MercyPreviewParam()]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
