using Core.Config;
using Core.Helpers.GitHub;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using Ui.Helper.BrowserHelpers;

namespace Ui.Helper;

public abstract class GitHubUiTestBase
{
    private const int ViewportWidth = 1440;
    private const int ViewportHeight = 900;
    private const int UiTimeoutMs = 15_000;

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private BrowserPw? _browserWrapper;

    // API cleanup against ui-sandbox (GITHUB_UI_PAT) by composition —
    // GitHubUiTestBase does not inherit GitHubTestBase
    protected UiSandboxCleanupHelper Cleanup { get; } = new();

    protected IBrowserContext Context => _context
        ?? throw new InvalidOperationException("Context not initialized — OpenContextAsync has not run");
    protected IPage Page => _page
        ?? throw new InvalidOperationException("Page not initialized — OpenContextAsync has not run");
    internal BrowserPw Browser => _browserWrapper
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
        _context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = storageState is not null && System.IO.File.Exists(storageState) ? storageState : null,
            ViewportSize = new ViewportSize { Width = ViewportWidth, Height = ViewportHeight }
        });
        await _context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true
        });

        _page = await _context.NewPageAsync();
        await _page.BringToFrontAsync();
        _page.SetDefaultTimeout(UiTimeoutMs);
        _page.SetDefaultNavigationTimeout(UiTimeoutMs);
        _browserWrapper = new BrowserPw(_page);
    }

    [TearDown]
    public async Task CloseContextAsync()
    {
        if (_context is null)
        {
            return;
        }

        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            var artifacts = ResolveArtifactsDirectory();
            var name = SanitizeFileName(TestContext.CurrentContext.Test.Name);
            await _page!.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(artifacts, $"{name}.png"),
                FullPage = true
            });
            await _context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(artifacts, $"{name}.zip")
            });
        }
        else
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
        while (dir is not null && !System.IO.File.Exists(Path.Combine(dir.FullName, "Gredja.slnx")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException(
                $"repository root (Gredja.slnx) not found from {AppContext.BaseDirectory}");
        }

        return dir.FullName;
    }

    private static string SanitizeFileName(string name) =>
        string.Concat(name.Where(character => !Path.GetInvalidFileNameChars().Contains(character)));
}
