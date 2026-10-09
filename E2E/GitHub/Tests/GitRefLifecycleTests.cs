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
public class GitRefLifecycleTests : GitHubE2ETestBase
{
    private const string RefPrefix = "audit-e2e-";

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
        var refName = $"{RefPrefix}{DataGenerator.RandomString(8)}";
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{RefsHeadsPrefix}{refName}", Sha = _baseSha },
            TestRepoParam());
        if (created.Data is not null)
        {
            _createdRefs.Add(refName);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);

        var afterCreate = await WaitHelper.WaitUntilAsync(
            () => Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, TestRepoParam()),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data!.Any(item => item.Name == refName));
        afterCreate.IsSuccess.Should().BeTrue(
            $"created ref must appear in the branches list;" +
            $" elapsed {afterCreate.Elapsed}, attempts {afterCreate.Attempts}," +
            $" last status {afterCreate.LastValue?.StatusCode}");

        var duplicate = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{RefsHeadsPrefix}{refName}", Sha = _baseSha },
            TestRepoParam());
        duplicate.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        var deleted = await Delete<object>(GitHubEndpoints.RepoGitRefById,
            [.. TestRepoParam(), .. GitRefParam($"{HeadsPrefix}{refName}")]);
        deleted.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        var afterDelete = await WaitHelper.WaitUntilAsync(
            () => Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, TestRepoParam()),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data!.All(item => item.Name != refName));
        afterDelete.IsSuccess.Should().BeTrue(
            $"deleted ref must disappear from the branches list;" +
            $" elapsed {afterDelete.Elapsed}, attempts {afterDelete.Attempts}," +
            $" last status {afterDelete.LastValue?.StatusCode}");
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
