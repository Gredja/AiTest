using Core.Config;
using Microsoft.Playwright;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Pages;

internal sealed class HomePage(IPage page)
{
    // the demo sometimes serves pages whose ESM bundles 404 (bootstrap never binds) —
    // a reload gets a fresh variant; a still-dead menu after the attempts fails honestly
    private const int MenuOpenAttempts = 3;
    private const int MenuSettleMs = 300;
    private const int NavigationAttempts = 2;
    private const int NavigationTimeoutMs = 30_000;
    private static readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

    internal Task GotoAsync(string? langCode = null)
    {
        var pathAndQuery = langCode is null
            ? MoodledemoEndpoints.HomePath
            : $"{MoodledemoEndpoints.HomePath}?{MoodledemoEndpoints.LangQueryKey}={langCode}";
        var expectedLang = langCode is null
            ? MoodledemoFixtures.English.Lang
            : MoodledemoFixtures.Home.Single(row => row.Code == langCode).Lang;

        return SiteNavigation.GotoVerifiedAsync(
            page,
            $"{MoodledemoEndpoints.BaseUrl}{pathAndQuery}",
            expectedLang);
    }

    internal Task GotoMyDashboardAsync(string expectedLang) =>
        SiteNavigation.GotoVerifiedAsync(
            page,
            $"{MoodledemoEndpoints.BaseUrl}{MoodledemoEndpoints.MyDashboardPath}",
            expectedLang);

    internal async Task OpenLanguageMenuAsync()
    {
        for (var attempt = 1; attempt <= MenuOpenAttempts; attempt++)
        {
            // Moodle binds the dropdown after its JS loads — click only once the page is idle
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _languageMenuToggle.ClickAsync();
            await Task.Delay(MenuSettleMs);

            if (await _languageMenu.IsVisibleAsync())
            {
                return;
            }

            if (attempt < MenuOpenAttempts)
            {
                await page.ReloadAsync(new PageReloadOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded
                });
                await Task.Delay(_retryDelay);
            }
        }

        await _languageMenu.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible
        });
    }

    internal ILocator LanguageLinks =>
        _languageMenu.Locator("a[role='menuitem'][href*='?lang=']");

    internal ILocator CurrentLanguageItem =>
        _languageMenu.Locator("a[role='menuitem'][aria-current='true']");

    internal async Task ClickLanguageAsync(string code)
    {
        for (var attempt = 1; attempt <= NavigationAttempts; attempt++)
        {
            if (attempt > 1)
            {
                // the previous click stalled mid-navigation — restart from a fresh page
                await GotoAsync();
                await OpenLanguageMenuAsync();
            }

            await _languageMenu
                .Locator($"a[role='menuitem'][href$='?{MoodledemoEndpoints.LangQueryKey}={code}']")
                .ClickAsync();

            try
            {
                await page.WaitForURLAsync(
                    url => url.Contains($"{MoodledemoEndpoints.LangQueryKey}={code}", StringComparison.Ordinal),
                    new PageWaitForURLOptions
                    {
                        Timeout = NavigationTimeoutMs,
                        WaitUntil = WaitUntilState.DOMContentLoaded
                    });
                return;
            }
            catch (TimeoutException) when (attempt < NavigationAttempts)
            {
                // the navigation did not reach DCL in time — the retry above starts over
            }
        }
    }

    internal Task<int> CountLanguageLinksAsync(string code) =>
        _languageMenu
            .Locator($"a[role='menuitem'][href$='?{MoodledemoEndpoints.LangQueryKey}={code}']")
            .CountAsync();

    internal async Task<string> CurrentLanguageTextAsync() =>
        (await _currentLanguage.TextContentAsync())?.Trim() ?? string.Empty;

    internal async Task OpenCalendarAsync()
    {
        await page
            .Locator($"a[role='menuitem'][href*='{MoodledemoEndpoints.CalendarPath}']")
            .ClickAsync();
        await page.WaitForURLAsync(
            url => url.Contains(MoodledemoEndpoints.CalendarPath, StringComparison.Ordinal),
            new PageWaitForURLOptions
            {
                Timeout = NavigationTimeoutMs,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
    }

    private readonly ILocator _languageMenuToggle = page.Locator("#lang-menu-toggle");
    private readonly ILocator _languageMenu = page.Locator("#lang-action-menu");
    private readonly ILocator _currentLanguage = page.Locator("#lang-menu-toggle .langbutton");
}
