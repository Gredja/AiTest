using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class GitRefLifecycleTests : GitHubE2ETestBase
{
    private const string RefPrefix = "audit-e2e-";
    private const int RandomNameLength = 8;

    private readonly List<string> _createdRefs = [];

    private string _baseSha = string.Empty;

    [OneTimeSetUp]
    public async Task ResolveBaseSha()
    {
        var branch = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);
        branch.ShouldHaveStatusCode(HttpStatusCode.OK);
        _baseSha = branch.Data!.Commit.Sha;
    }

    [Test]
    [Category("Regression")]
    [Description("E2E-7 Git ref lifecycle: create branch → visible in list → duplicate 422 → delete → gone")]
    public async Task GitRefLifecycle_Create_VerifyDuplicate_Delete()
    {
        var refName = $"{RefPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        await CreateBranchTrackedAsync(refName, _baseSha, _createdRefs);
        await WaitUntilBranchVisibleAsync(refName, present: true);

        var duplicate = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{RefsHeadsPrefix}{refName}", Sha = _baseSha },
            TestRepoParam());
        duplicate.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        var deleted = await Delete<object>(GitHubEndpoints.RepoGitRefById,
            [.. TestRepoParam(), .. GitRefParam($"{HeadsPrefix}{refName}")]);
        deleted.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        await WaitUntilBranchVisibleAsync(refName, present: false);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var refPath in _createdRefs)
        {
            await CleanupGitRefAsync(refPath);
        }
    }
}
