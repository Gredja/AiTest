using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;
using static Core.Helpers.TestDataConstants;

namespace Api.GitHub.Releases;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetReleaseDetailTests : GitHubTestBase
{
    private const string ReleaseTagPrefix = "phase2-release-";

    private string _tagName = string.Empty;
    private long _releaseId;

    [OneTimeSetUp]
    public async Task CreateFixtureRelease()
    {
        _tagName = $"{ReleaseTagPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var created = await Post<CreateReleaseModelRequest, ReleaseModelResponse>(
            GitHubEndpoints.RepoReleases,
            new CreateReleaseModelRequest { TagName = _tagName, Name = "release detail fixture" },
            TestRepoParam());
        if (created.Data is not null)
        {
            _releaseId = created.Data.Id;
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Test]
    [Category("HealthCheck")]
    [Description("15c.1 GET /releases/latest returns 200 OK")]
    public async Task GetLatestRelease_ReturnsOk()
    {
        var response = await Get<ReleaseModelResponse>(GitHubEndpoints.RepoReleasesLatest, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);

        // /releases/latest may legitimately return a release published outside the suite that is
        // newer than this fixture — the fixture itself is verified via GET /releases/tags/{tag} (15d.1)
        response.Data!.TagName.Should().NotBeNullOrWhiteSpace("latest release must carry a tag");
    }

    [Test]
    [Category("Negative")]
    [Description("15c.2 Non-existent repo returns 404")]
    public async Task GetLatestRelease_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<ReleaseModelResponse>(GitHubEndpoints.RepoReleasesLatest,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("HealthCheck")]
    [Description("15d.1 GET /releases/tags/{release_tag} returns 200 OK")]
    public async Task GetReleaseByTag_ReturnsOk()
    {
        var response = await Get<ReleaseModelResponse>(GitHubEndpoints.RepoReleaseByTag,
            [.. TestRepoParam(), .. ReleaseTagParam(_tagName)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.TagName.Should().Be(_tagName);
    }

    [Test]
    [Category("Negative")]
    [Description("15d.2 Non-existent tag returns 404")]
    public async Task GetReleaseByTag_NonExistentTag_ReturnsNotFound()
    {
        var response = await Get<ReleaseModelResponse>(GitHubEndpoints.RepoReleaseByTag,
            [.. TestRepoParam(), .. ReleaseTagParam(GitHubEndpoints.NonExistentTagName)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await CleanupReleaseAsync(_releaseId);
        await CleanupGitRefAsync($"{GitHubEndpoints.TagsPrefix}{_tagName}");
    }
}
