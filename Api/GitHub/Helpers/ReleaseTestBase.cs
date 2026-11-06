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

    protected async Task<(long Id, string TagName)> CreateReleaseFixtureAsync()
    {
        var request = ReleaseGenerator.CreateRandom(ReleaseTagPrefix, "release fixture");
        var tagName = request.TagName;
        var created = await Post<CreateReleaseModelRequest, ReleaseModelResponse>(
            GitHubEndpoints.RepoReleases,
            request,
            TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdReleases.Add((created.Data!.Id, tagName));

        return (created.Data.Id, tagName);
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
