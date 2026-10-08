using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Contributors;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetContributorsTests : GitHubTestBase
{
    private const int SingleItemPageSize = 1;
    private const string SpecialCharsSegment = "sp@ec!al";

    [Test]
    [Category("HealthCheck")]
    [Description("12.1 GET /contributors returns 200 OK")]
    public async Task GetContributors_ReturnsOk()
    {
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("12.2 Response matches expected contract")]
    public async Task GetContributors_ResponseMatchesContract()
    {
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty("repo must keep data — Entry Criteria, documentation/GitHubTestingStructure.md");
        response.Data.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("12.3 Fields: login non-empty, id > 0, contributions > 0")]
    public async Task GetContributors_HasValidFields()
    {
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        foreach (var contributor in response.Data!)
        {
            contributor.ShouldHaveValidFields();
            contributor.Contributions.Should().BeGreaterThan(0, "every contributor must have positive contributions");
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("12.4 Content-Type is application/json")]
    public async Task GetContributors_ContentTypeIsJson()
    {
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            TestRepoParam());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("12.5 Returns non-empty contributor list")]
    public async Task GetContributors_ReturnsNonEmptyList()
    {
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty("repo must have at least one contributor");
    }

    [Test]
    [Category("Regression")]
    [Description("12.6 Items sorted by contributions descending")]
    public async Task GetContributors_SortedByContributionsDescending()
    {
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Select(contributor => contributor.Contributions)
            .Should().BeInDescendingOrder("contributors must be sorted by contributions descending");
    }

    [Test]
    [Category("Regression")]
    [Description("12.7 Login values are unique")]
    public async Task GetContributors_LoginsAreUnique()
    {
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Select(contributor => contributor.Login).Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Performance")]
    [Description("12.8 Response time < 5 seconds")]
    public async Task GetContributors_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            TestRepoParam());
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("12.9 per_page=1 returns at most 1 contributor")]
    public async Task GetContributors_PerPageOne_ReturnsAtMostOne()
    {
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            [.. TestRepoParam(), .. PaginationParams(FirstPage, SingleItemPageSize)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(SingleItemPageSize);
    }

    [Test]
    [Category("Negative")]
    [Description("12.10 Non-existent repo returns 404")]
    public async Task GetContributors_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("12.11 Non-existent owner returns 404")]
    public async Task GetContributors_NonExistentOwner_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            RepoParam(GitHubEndpoints.NonExistentUser, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("12.12 Invalid token returns 401")]
    public async Task GetContributors_InvalidToken_ReturnsUnauthorized()
    {
        var response = await ExecuteWithAuthorization(
            GitHubEndpoints.RepoContributors, InvalidAuthorization, TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("12.13 Owner with special chars returns 404")]
    public async Task GetContributors_OwnerWithSpecialChars_ReturnsNotFound()
    {
        var (_, repo) = ParseRepo();

        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            RepoParam(SpecialCharsSegment, repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("12.14 Repo with special chars returns 404")]
    public async Task GetContributors_SpecialCharsRepo_ReturnsNotFound()
    {
        var (owner, _) = ParseRepo();

        var response = await Get<List<ContributorModelResponse>>(GitHubEndpoints.RepoContributors,
            RepoParam(owner, SpecialCharsSegment));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
