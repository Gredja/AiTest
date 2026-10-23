using Core.Config;
using Core.Logging;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using Ui.Helpers;

namespace Ui.Moodledemo.Tests.Support;

public abstract class MoodledemoTestBase
{
    private const int ViewportWidth = 1440;
    private const int ViewportHeight = 900;
    private const string PinnedLocale = "en-US";
    private const string MoodledemoArtifactsDirectory = "moodledemo-artifacts";
    internal const int UiTimeoutMs = 15_000;

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private MoodledemoBrowser? _browserWrapper;

    internal MoodledemoBrowser Browser => _browserWrapper
        ?? throw new InvalidOperationException("Browser wrapper not initialized — OpenContextAsync has not run");

    // Credential-entry fixtures must not record DOM snapshots — a failed run would persist
    // the typed demo password in the trace zip (same guard as GitHubUiTestBase)
    protected virtual bool IsCredentialEntryTest => false;

    [OneTimeSetUp]
    public async Task LaunchBrowserAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = TestConfig.IsUiHeadless
        });
    }

    // The demo site has no per-user state to carry — every context is anonymous by design
    // The site picks the default language from Accept-Language — pin the locale so every
    // anonymous visit deterministically starts as en_us (Playwright's own default is en-US,
    // but the explicit value documents the dependency)
    [SetUp]
    public async Task OpenContextAsync()
    {
        _context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = ViewportWidth, Height = ViewportHeight },
            Locale = PinnedLocale
        });
        if (!IsCredentialEntryTest)
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
        _browserWrapper = new MoodledemoBrowser(_page);
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
            var failedName = ArtifactNamingHelper.SanitizeFileName(TestContext.CurrentContext.Test.Name);
            await _page!.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(artifacts, $"{failedName}.png"),
                FullPage = true
            });
        }

        if (!IsCredentialEntryTest)
        {
            if (isFailed)
            {
                var traceName = ArtifactNamingHelper.SanitizeFileName(TestContext.CurrentContext.Test.Name);
                await _context.Tracing.StopAsync(new TracingStopOptions
                {
                    Path = Path.Combine(ResolveArtifactsDirectory(), $"{traceName}.zip")
                });
            }
            else
            {
                await _context.Tracing.StopAsync();
            }
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

    private static string ResolveArtifactsDirectory()
    {
        var directory = Path.Combine(TestRunWorkspace.Resolve(), MoodledemoArtifactsDirectory);
        Directory.CreateDirectory(directory);

        return directory;
    }
}
