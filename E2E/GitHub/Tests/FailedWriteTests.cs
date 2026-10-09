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
public class FailedWriteTests : GitHubE2ETestBase
{
    private const int TitleRandomLength = 8;

    private readonly List<string> _createdBranches = [];

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
    [Description("E2E-9 Failed write leaves no side effects: PR without commits → 422 → no PR leaked")]
    public async Task FailedWrite_NoCommitsPr_LeavesNoSideEffects()
    {
        var branchName = $"audit-e2e-neg-{DataGenerator.RandomString(8)}";
        var branch = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{RefsHeadsPrefix}{branchName}", Sha = _baseSha },
            TestRepoParam());
        if (branch.Data is not null)
        {
            _createdBranches.Add(branchName);
        }

        branch.ShouldHaveStatusCode(HttpStatusCode.Created);

        var branchVisible = await WaitHelper.WaitUntilAsync(
            () => Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, TestRepoParam()),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data!.Any(item => item.Name == branchName));
        branchVisible.IsSuccess.Should().BeTrue(
            $"created branch must appear in the branches list;" +
            $" elapsed {branchVisible.Elapsed}, attempts {branchVisible.Attempts}," +
            $" last status {branchVisible.LastValue?.StatusCode}");

        var pullRequest = await Post<CreatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequests,
            new CreatePullRequestModelRequest
            {
                Title = $"Failed {DataGenerator.RandomString(TitleRandomLength)}",
                Head = branchName,
                Base = GitHubEndpoints.DefaultBranch
            },
            TestRepoParam());
        pullRequest.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        var pulls = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        pulls.ShouldHaveStatusCode(HttpStatusCode.OK);
        pulls.Data.Should().NotContain(pull => pull.Head.Ref == branchName,
            "rejected PR create must not leak a pull request");

        var branches = await Get<List<BranchModelResponse>>(GitHubEndpoints.RepoBranches, TestRepoParam());
        branches.ShouldHaveStatusCode(HttpStatusCode.OK);
        branches.Data.Should().Contain(item => item.Name == branchName,
            "failed PR create must leave the source branch intact");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var branchName in _createdBranches)
        {
            await CleanupGitRefAsync(branchName);
        }
    }
}
