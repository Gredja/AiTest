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
    private const int MenuIdleTimeoutMs = 5_000;
    private const int ToggleClickTimeoutMs = 5_000;
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
            // state check before every re-click: a menu that rendered late must never be
            // toggled shut by the retry
            if (attempt > 1 && await LanguageLinks.First.IsVisibleAsync())
            {
                return;
            }

            try
            {
                // Moodle binds the dropdown after its JS loads — click only once idle
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
                {
                    Timeout = MenuIdleTimeoutMs
                });
            }
            catch (TimeoutException)
            {
                // a hung request (observed on the demo) keeps the page off-idle — proceed;
                // the visibility check below decides whether the click is worth anything
            }

            try
            {
                // bounded click timeout: a degraded variant may not render the toggle at
                // all — reload and retry instead of burning the whole 15s action budget
                await LanguageMenuToggle.ClickAsync(new LocatorClickOptions
                {
                    Timeout = ToggleClickTimeoutMs
                });
            }
            catch (TimeoutException) when (attempt < MenuOpenAttempts)
            {
                await ReloadForRetryAsync();
                continue;
            }

            if (await IsLanguageMenuVisibleAsync(MenuSettleMs))
            {
                return;
            }

            if (attempt < MenuOpenAttempts)
            {
                await ReloadForRetryAsync();
            }
        }

        await LanguageLinks.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible
        });
    }

    private async Task ReloadForRetryAsync()
    {
        await page.ReloadAsync(new PageReloadOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await Task.Delay(_retryDelay);
    }

    private async Task<bool> IsLanguageMenuVisibleAsync(int timeoutMs)
    {
        try
        {
            // bounded wait for the menu to render after the click — replaces the fixed settle
            // sleep: the post-condition (a visible language link) IS the outcome of the click
            await LanguageLinks.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });

            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    internal ILocator LanguageLinks =>
        page.Locator(LanguageLinksSelector);

    internal ILocator CurrentLanguageItem =>
        page.Locator(CurrentLanguageSelector);

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

            await page.Locator(LanguageItemSelector(code))
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
        page.Locator(LanguageItemSelector(code))
            .CountAsync();

    internal async Task<string> CurrentLanguageTextAsync() =>
        (await LanguageMenuToggle.TextContentAsync())?.Trim() ?? string.Empty;

    internal async Task OpenCalendarAsync()
    {
        await page.Locator(CalendarItemSelector)
            .ClickAsync();
        await page.WaitForURLAsync(
            url => url.Contains(MoodledemoEndpoints.CalendarPath, StringComparison.Ordinal),
            new PageWaitForURLOptions
            {
                Timeout = NavigationTimeoutMs,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
    }

    internal Task GotoCalendarAsync(string? langCode = null)
    {
        var pathAndQuery = langCode is null
            ? $"{MoodledemoEndpoints.CalendarPath}?view=month"
            : $"{MoodledemoEndpoints.CalendarPath}?view=month&{MoodledemoEndpoints.LangQueryKey}={langCode}";
        var expectedLang = langCode is null
            ? MoodledemoFixtures.English.Lang
            : MoodledemoFixtures.Home.Single(row => row.Code == langCode).Lang;

        return SiteNavigation.GotoVerifiedAsync(
            page,
            $"{MoodledemoEndpoints.BaseUrl}{pathAndQuery}",
            expectedLang);
    }

    internal async Task<IReadOnlyList<string>> ReadNavLabelsAsync()
    {
        var texts = await page.Locator(NavLabelSelector).AllTextContentsAsync();

        return texts
            .Select(text => TextPatterns.CollapseWhitespace(text))
            .Where(text => !string.IsNullOrEmpty(text))
            .ToList();
    }

    internal async Task<string> ReadCalendarControlsTextAsync()
    {
        var controls = page.Locator(CalendarControlsSelector).First;
        var text = await controls.TextContentAsync();

        return TextPatterns.CollapseWhitespace(text);
    }

    internal async Task<(long ScrollWidth, long ClientWidth)> ReadPageOverflowAsync()
    {
        var scrollWidth = await page.EvaluateAsync<long>(
            "(document.querySelector('#page-content') || document.body).scrollWidth");
        var clientWidth = await page.EvaluateAsync<long>(
            "(document.querySelector('#page-content') || document.body).clientWidth");

        return (scrollWidth, clientWidth);
    }

    internal async Task<string> ReadBodyDirectionAsync() =>
        await page.EvaluateAsync<string>("getComputedStyle(document.body).direction");

    internal async Task<string> ReadNavDirectionAsync()
    {
        var nav = page.Locator("nav").First;

        return await nav.EvaluateAsync<string>("el => getComputedStyle(el).direction");
    }

    // The guest footer offers a log-in link — named so tests never build footer selectors
    internal ILocator FooterLoginLinks =>
        page.Locator(FooterLoginSelector);

    // The toggle's accessible name is localized (en "Language" / de "Sprache" / ru "Язык",
    // dumps 2026-10-23) and it exposes no stable ARIA attribute at rest — its text always
    // carries the current-language marker "(code)", which is the locale-independent identity
    private ILocator LanguageMenuToggle =>
        page.GetByRole(AriaRole.Button).Filter(new LocatorFilterOptions { HasTextString = "(" });

    // The structural selectors below have no accessible-name path (live dump 2026-10-22):
    // language items are addressed by their href contract (?lang=code — the fixture-driven
    // identity), nav labels by the only scoping that returns the full list (no navigation
    // landmark in the accessibility tree; mixed menuitem/link roles), calendar controls
    // by their class (no landmark/role exposes the block)
    private const string LanguageLinksSelector =
        $"a[role='menuitem'][href*='?{MoodledemoEndpoints.LangQueryKey}=']";

    private const string CurrentLanguageSelector =
        "a[role='menuitem'][aria-current='true']";

    private const string NavLabelSelector = "nav a.nav-link, nav .nav-link";

    private const string CalendarControlsSelector = ".calendar-controls";

    private const string CalendarItemSelector =
        $"a[role='menuitem'][href*='{MoodledemoEndpoints.CalendarPath}']";

    private const string FooterLoginSelector =
        $"footer a[href*='{MoodledemoEndpoints.LoginPath}']";

    private static string LanguageItemSelector(string code) =>
        $"a[role='menuitem'][href$='?{MoodledemoEndpoints.LangQueryKey}={code}']";
}
