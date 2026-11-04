using Core.Config;
using Core.Generators.GitHub;
using Core.Helpers;
using Core.Helpers.GitHub;
using Core.Models.GitHub;
using System.Net;

namespace Api.GitHub.Helpers;

public abstract class ReleaseTestBase : GitHubTestBase
{
    private const string ReleaseTagPrefix = "phase2-rel-";

    private readonly List<(long Id, string TagName)> _createdReleases = [];

    // Register-then-assert: the release joins the cleanup registry before any status check,
    // so a failed POST still leaves nothing to leak in the sandbox repo.
    protected async Task<(long Id, string TagName)> CreateReleaseFixtureAsync()
    {
        var request = ReleaseGenerator.CreateRandom(ReleaseTagPrefix, "release fixture");
        var tagName = request.TagName;
        var created = await Post<CreateReleaseModelRequest, ReleaseModelResponse>(
            GitHubEndpoints.RepoReleases,
            request,
            TestRepoParam());

        if (created.Data is not null)
        {
            _createdReleases.Add((created.Data.Id, tagName));
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);

        return (created.Data!.Id, tagName);
    }

    protected async Task CleanupReleaseFixturesAsync()
    {
        foreach (var (releaseId, tagName) in _createdReleases)
        {
            await CleanupReleaseAsync(releaseId);
            await CleanupGitRefAsync($"{GitHubEndpoints.TagsPrefix}{tagName}");
        }
    }
}
