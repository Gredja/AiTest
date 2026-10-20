using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using Core.Models.GitHub;
using Core.Models.Generic;
using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using System.Net;

namespace Ui.Github.Tests.Support;

public abstract class GitHubUiTestBase
{
    private const int ViewportWidth = 1440;
    private const int ViewportHeight = 900;
    internal const int UiTimeoutMs = 15_000;

    private const string ScratchBranchPrefix = "ui-scratch-";
    private const string ScratchBaseBranchPrefix = "ui-scratch-base-";
    internal const string ScratchFileName = "ui-scratch.txt";

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private BrowserPlaywright? _browserWrapper;

    protected UiSandboxCleanupHelper Cleanup { get; } = new();

    protected IBrowserContext Context => _context
        ?? throw new InvalidOperationException("Context not initialized — OpenContextAsync has not run");
    protected IPage Page => _page
        ?? throw new InvalidOperationException("Page not initialized — OpenContextAsync has not run");
    internal BrowserPlaywright Browser => _browserWrapper
        ?? throw new InvalidOperationException("Browser wrapper not initialized — OpenContextAsync has not run");

    protected virtual bool IsStorageStateEnabled => true;

    [OneTimeSetUp]
    public async Task LaunchBrowserAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = TestConfig.IsUiHeadless
        });
    }

    [SetUp]
    public async Task OpenContextAsync()
    {
        var storageState = IsStorageStateEnabled ? ResolveStorageStatePath() : null;

        // Missing state must fail here with the real reason — otherwise the test silently
        // continues anonymous and fails much later on the wrong screen
        if (storageState is not null && !System.IO.File.Exists(storageState))
        {
            throw new InvalidOperationException(
                $"storageState file not found: {storageState} — run LoginSignInTests once to create it");
        }

        _context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = storageState is not null && System.IO.File.Exists(storageState) ? storageState : null,
            ViewportSize = new ViewportSize { Width = ViewportWidth, Height = ViewportHeight }
        });

        // Credential-entry tests (IsStorageStateEnabled = false) must not record traces:
        // DOM snapshots and network captures would persist the typed password in ui-artifacts/*.zip
        if (IsStorageStateEnabled)
        {
            await _context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true
            });
        }

        _page = await _context.NewPageAsync();
        await _page.BringToFrontAsync();
        _page.SetDefaultTimeout(UiTimeoutMs);
        _page.SetDefaultNavigationTimeout(UiTimeoutMs);
        _browserWrapper = new BrowserPlaywright(_page);
    }

    [TearDown]
    public async Task CloseContextAsync()
    {
        if (_context is null)
        {
            return;
        }

        var isFailed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;

        if (isFailed)
        {
            var artifacts = ResolveArtifactsDirectory();
            var failedName = SanitizeFileName(TestContext.CurrentContext.Test.Name);
            await _page!.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(artifacts, $"{failedName}.png"),
                FullPage = true
            });

            if (IsStorageStateEnabled)
            {
                await _context.Tracing.StopAsync(new TracingStopOptions
                {
                    Path = Path.Combine(artifacts, $"{failedName}.zip")
                });
            }
        }
        else if (IsStorageStateEnabled)
        {
            await _context.Tracing.StopAsync();
        }

        await _context.CloseAsync();
        _context = null;
        _page = null;
        _browserWrapper = null;
    }

    [OneTimeTearDown]
    public async Task CloseBrowserAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }

    protected async Task SaveStorageStateAsync()
    {
        var stateFile = new System.IO.FileInfo(ResolveStorageStatePath());
        stateFile.Directory?.Create();
        await Context.StorageStateAsync(new BrowserContextStorageStateOptions
        {
            Path = stateFile.FullName
        });
    }

    protected static List<RequestDictionaryModel> UiSandboxRepoParams()
    {
        var parts = TestConfig.UiTestRepo.Split('/');

        return GitHubParamHelper.RepoParam(parts[0], parts[1]);
    }

    protected async Task<int> CreateIssueViaApiAsync(List<int> createdIssueNumbers, bool isClosed = false, string? title = null)
    {
        title ??= $"API setup {DataGenerator.RandomString(TestDataConstants.TitleRandomLength)}";
        var created = await Cleanup.Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues,
            new CreateIssueModelRequest { Title = title },
            UiSandboxRepoParams());
        if (created.Data is not null)
        {
            createdIssueNumbers.Add(created.Data.Number);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        var issueNumber = created.Data!.Number;

        if (isClosed)
        {
            await Cleanup.CleanupIssueAsync(issueNumber);
        }

        return issueNumber;
    }

    // The UI never exposes the comment id, but cleanup deletes by id — read it back via API before
    // the assert (register-then-assert), polling over the read-after-write indexing lag
    protected async Task RegisterCommentIdAsync(int issueNumber, string body, List<long> createdCommentIds)
    {
        var found = await WaitHelper.WaitUntilAsync(
            async () =>
            {
                var response = await Cleanup.Get<List<CommentModelResponse>>(
                    GitHubEndpoints.RepoIssueComments,
                    [.. UiSandboxRepoParams(), .. GitHubParamHelper.IssueNumberParam(issueNumber)]);

                return response.Data?.FirstOrDefault(comment => comment.Body == body);
            },
            comment => comment is not null);

        found.LastValue.Should().NotBeNull(
            $"comment must be readable via API for cleanup (attempts={found.Attempts}, elapsed={found.Elapsed})");
        createdCommentIds.Add(found.LastValue!.Id);
    }

    protected async Task<WaitResult<bool>> WaitForIssueLabelAsync(int issueNumber, string labelName, bool hasLabel) =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                var response = await Cleanup.Get<IssueModelResponse>(
                    GitHubEndpoints.RepoIssueById,
                    [.. UiSandboxRepoParams(), .. GitHubParamHelper.IssueNumberParam(issueNumber)]);
                var found = response.Data?.Labels?.Any(label => label.Name == labelName) ?? false;

                return found == hasLabel;
            },
            matches => matches);

    protected async Task<string> CreateBranchViaApiAsync(string branchName, List<string> createdBranches)
    {
        var mainSha = await ResolveUiSandboxMainShaAsync();
        var created = await Cleanup.Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{GitHubEndpoints.RefsHeadsPrefix}{branchName}", Sha = mainSha },
            UiSandboxRepoParams());
        createdBranches.Add(branchName);
        created.ShouldHaveStatusCode(HttpStatusCode.Created);

        return created.Data!.Ref;
    }

    protected async Task<string> CreateFolderBranchViaApiAsync(string branchName, string folderName, List<string> createdBranches)
    {
        var mainSha = await ResolveUiSandboxMainShaAsync();
        var commitSha = await CreateScratchCommitViaApiAsync(mainSha, $"{folderName}/{ScratchFileName}");
        await CreateBranchRefViaApiAsync(branchName, commitSha);
        createdBranches.Add(branchName);

        return branchName;
    }

    protected async Task<int> ResolveNonExistentIssueNumberAsync()
    {
        var response = await Cleanup.Get<List<IssueModelResponse>>(
            GitHubEndpoints.RepoIssues,
            [.. UiSandboxRepoParams(), .. GitHubParamHelper.StateParam(GitHubEndpoints.StateAll), .. GitHubParamHelper.PerPageParam(1)]);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);

        // A pristine sandbox has no issues to derive the id from — 0 is always invalid
        if (response.Data is null || response.Data.Count == 0)
        {
            return GitHubEndpoints.NonExistentStaticId;
        }

        return response.Data[0].Number + GitHubEndpoints.NonExistentIdOffset;
    }

    protected async Task<int> ResolveNonExistentPullNumberAsync()
    {
        var response = await Cleanup.Get<List<PullRequestModelResponse>>(
            GitHubEndpoints.RepoPullRequests,
            [.. UiSandboxRepoParams(), .. GitHubParamHelper.StateParam(GitHubEndpoints.StateAll), .. GitHubParamHelper.PerPageParam(1)]);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);

        // A pristine sandbox has no pull requests to derive the id from — 0 is always invalid
        if (response.Data is null || response.Data.Count == 0)
        {
            return GitHubEndpoints.NonExistentStaticId;
        }

        return response.Data[0].Number + GitHubEndpoints.NonExistentIdOffset;
    }

    protected async Task<(string HeadBranch, string BaseBranch)> CreateDivergentBranchesViaApiAsync(List<string> createdBranches)
    {
        var headBranch = $"{ScratchBranchPrefix}{DataGenerator.RandomString(TestDataConstants.RandomNameLength)}";
        var baseBranch = $"{ScratchBaseBranchPrefix}{DataGenerator.RandomString(TestDataConstants.RandomNameLength)}";
        var mainSha = await ResolveUiSandboxMainShaAsync();
        var commitSha = await CreateScratchCommitViaApiAsync(mainSha);
        await CreateBranchRefViaApiAsync(headBranch, commitSha);
        createdBranches.Add(headBranch);
        await CreateBranchRefViaApiAsync(baseBranch, mainSha);
        createdBranches.Add(baseBranch);

        return (headBranch, baseBranch);
    }

    protected async Task<(int Number, string HeadBranch, string BaseBranch)> CreateScratchPullRequestViaApiAsync(
        string title,
        List<(int Number, string HeadBranch, string BaseBranch)> createdPullRequests,
        List<string> createdBranches)
    {
        var (headBranch, baseBranch) = await CreateDivergentBranchesViaApiAsync(createdBranches);
        var pullRequest = await Cleanup.Post<CreatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequests,
            new CreatePullRequestModelRequest { Title = title, Head = headBranch, Base = baseBranch },
            UiSandboxRepoParams());
        if (pullRequest.Data is not null)
        {
            createdPullRequests.Add((pullRequest.Data.Number, headBranch, baseBranch));
        }

        pullRequest.ShouldHaveStatusCode(HttpStatusCode.Created);

        return (pullRequest.Data.Number, headBranch, baseBranch);
    }

    protected async Task ClosePullRequestViaApiAsync(int pullNumber)
    {
        var closed = await Cleanup.Patch<UpdatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequestById,
            new UpdatePullRequestModelRequest { State = GitHubEndpoints.StateClosed },
            [.. UiSandboxRepoParams(), .. GitHubParamHelper.PullRequestNumberParam(pullNumber)]);
        closed.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    private async Task<string> ResolveUiSandboxMainShaAsync()
    {
        var mainBranch = await Cleanup.Get<BranchModelResponse>(
            GitHubEndpoints.RepoBranchByName,
            [.. UiSandboxRepoParams(), .. GitHubParamHelper.BranchNameParam(GitHubEndpoints.DefaultBranch)]);
        mainBranch.ShouldHaveStatusCode(HttpStatusCode.OK);

        return mainBranch.Data!.Commit.Sha;
    }

    private async Task<string> CreateScratchCommitViaApiAsync(string mainSha, string treePath = ScratchFileName)
    {
        var mainCommit = await Cleanup.Get<GitCommitModelResponse>(
            GitHubEndpoints.RepoGitCommitsById,
            [.. UiSandboxRepoParams(), .. GitHubParamHelper.GitCommitParam(mainSha)]);
        mainCommit.ShouldHaveStatusCode(HttpStatusCode.OK);

        var blob = await Cleanup.Post<CreateGitBlobModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs,
            new CreateGitBlobModelRequest { Content = $"ui scratch {DataGenerator.RandomString(TestDataConstants.RandomNameLength)}" },
            UiSandboxRepoParams());
        blob.ShouldHaveStatusCode(HttpStatusCode.Created);

        var tree = await Cleanup.Post<CreateGitTreeModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitTrees,
            new CreateGitTreeModelRequest
            {
                BaseTree = mainCommit.Data!.Tree.Sha,
                Tree =
                [
                    new GitTreeEntry
                    {
                        Path = treePath,
                        Mode = GitHubEndpoints.FileMode644,
                        Type = GitHubEndpoints.ObjectTypeBlob,
                        Sha = blob.Data!.Sha
                    }
                ]
            },
            UiSandboxRepoParams());
        tree.ShouldHaveStatusCode(HttpStatusCode.Created);

        var commit = await Cleanup.Post<CreateGitCommitModelRequest, GitCommitModelResponse>(
            GitHubEndpoints.RepoGitCommits,
            new CreateGitCommitModelRequest
            {
                Message = $"ui scratch {DataGenerator.RandomString(TestDataConstants.RandomNameLength)}",
                Tree = tree.Data!.Sha,
                Parents = [mainSha]
            },
            UiSandboxRepoParams());
        commit.ShouldHaveStatusCode(HttpStatusCode.Created);

        return commit.Data!.Sha;
    }

    private async Task CreateBranchRefViaApiAsync(string branchName, string sha)
    {
        var created = await Cleanup.Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{GitHubEndpoints.RefsHeadsPrefix}{branchName}", Sha = sha },
            UiSandboxRepoParams());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    // storageState and artifacts are resolved against the repository root so relative
    // config values work from bin/Debug output directories
    private static string ResolveStorageStatePath()
    {
        var statePath = TestConfig.UiStatePath;

        return Path.IsPathRooted(statePath)
            ? statePath
            : Path.Combine(ResolveRepositoryRoot(), statePath);
    }

    private static string ResolveArtifactsDirectory()
    {
        var directory = Path.Combine(ResolveRepositoryRoot(), "TestResults", "ui-artifacts");
        Directory.CreateDirectory(directory);

        return directory;
    }

    private static string ResolveRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !System.IO.File.Exists(Path.Combine(dir.FullName, TestConfig.SolutionFileName)))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException(
                $"repository root ({TestConfig.SolutionFileName}) not found from {AppContext.BaseDirectory}");
        }

        return dir.FullName;
    }

    private static string SanitizeFileName(string name) =>
        string.Concat(name.Where(character => !Path.GetInvalidFileNameChars().Contains(character)));
}
