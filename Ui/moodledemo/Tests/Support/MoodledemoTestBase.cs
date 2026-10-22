using Core.Config;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace Ui.Moodledemo.Tests.Support;

public abstract class MoodledemoTestBase
{
    private const int ViewportWidth = 1440;
    private const int ViewportHeight = 900;
    internal const int UiTimeoutMs = 15_000;

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private MoodledemoBrowser? _browserWrapper;

    internal MoodledemoBrowser Browser => _browserWrapper
        ?? throw new InvalidOperationException("Browser wrapper not initialized — OpenContextAsync has not run");

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
            Locale = "en-US"
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
        _browserWrapper = new MoodledemoBrowser(_page);
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
            var failedName = SanitizeFileName(TestContext.CurrentContext.Test.Name);
            await _page!.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(artifacts, $"{failedName}.png"),
                FullPage = true
            });
            await _context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(artifacts, $"{failedName}.zip")
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

    private static string ResolveArtifactsDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "GredjaTestRun", "moodledemo-artifacts");
        Directory.CreateDirectory(directory);

        return directory;
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalid, '_');
        }

        return name;
    }
}
