using Core.Config;
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

    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    internal BrowserPw Browser { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task LaunchBrowserAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = TestConfig.UiHeadless
        });
    }

    [SetUp]
    public async Task OpenContextAsync()
    {
        var storageState = ResolveStorageStatePath();
        Context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = System.IO.File.Exists(storageState) ? storageState : null,
            ViewportSize = new ViewportSize { Width = ViewportWidth, Height = ViewportHeight }
        });
        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true
        });

        Page = await Context.NewPageAsync();
        Page.SetDefaultTimeout(UiTimeoutMs);
        Page.SetDefaultNavigationTimeout(UiTimeoutMs);
        Browser = new BrowserPw(Page);
    }

    [TearDown]
    public async Task CloseContextAsync()
    {
        if (Context is null)
        {
            return;
        }

        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            var artifacts = ResolveArtifactsDirectory();
            var name = SanitizeFileName(TestContext.CurrentContext.Test.Name);
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(artifacts, $"{name}.png"),
                FullPage = true
            });
            await Context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(artifacts, $"{name}.zip")
            });
        }
        else
        {
            await Context.Tracing.StopAsync();
        }

        await Context.CloseAsync();
        Context = null!;
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
