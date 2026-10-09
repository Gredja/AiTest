using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class PullRequestFlowTests : GitHubE2ETestBase
{
    private const int TitleRandomLength = 8;

    private readonly List<(int Number, string HeadBranch, string BaseBranch)> _createdPullRequests = [];

    [Test]
    [Category("Regression")]
    [Description("E2E-2 Pull request flow: create branch → create PR → verify → merge → verify merged")]
    public async Task PullRequestFlow_Create_Verify_Merge()
    {
        var title = $"E2E {DataGenerator.RandomString(TitleRandomLength)}";
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(title);
        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var created = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(pullNumber));
        created.ShouldHaveStatusCode(HttpStatusCode.OK);
        created.Data!.State.Should().Be(GitHubEndpoints.StateOpen, "freshly created PR must be open");
        created.Data.Title.Should().Be(title, "PR must echo the requested title");

        var merged = await Put<Dictionary<string, object>, object>(
            GitHubEndpoints.RepoPullRequestMerge, new Dictionary<string, object>(), PullParams(pullNumber));
        merged.ShouldHaveStatusCode(HttpStatusCode.OK);
        merged.Data.Should().NotBeNull();

        var pulls = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        pulls.ShouldHaveStatusCode(HttpStatusCode.OK);
        var mergedPull = pulls.Data!.FirstOrDefault(pull => pull.Number == pullNumber);
        mergedPull.Should().NotBeNull("created PR must be listed after merge");
        mergedPull!.State.Should().Be(GitHubEndpoints.StateClosed, "merged PR must be closed");
        mergedPull.MergedAt.Should().NotBeNull("merged PR must carry merged_at");
        mergedPull.Head.Ref.Should().Be(headBranch, "merged PR must keep the created head branch");
        mergedPull.Base.Ref.Should().Be(baseBranch, "merged PR must keep the scratch base branch");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var (number, headBranch, baseBranch) in _createdPullRequests)
        {
            await CleanupPullRequestAsync(number, headBranch, baseBranch);
        }
    }
}
