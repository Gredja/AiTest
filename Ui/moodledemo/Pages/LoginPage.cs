using Core.Config;
using Microsoft.Playwright;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Pages;

internal sealed class LoginPage(IPage page)
{
    // the demo's post-login redirect was observed stalling for whole minutes — a fresh
    // attempt rides out the degraded window (observed: two consecutive 30 s dead redirects)
    private const int LoginNavigationTimeoutMs = 30_000;
    private const int LoginAttempts = 3;

    internal Task GotoAsync(string? langCode = null)
    {
        var url = langCode is null
            ? $"{MoodledemoEndpoints.BaseUrl}{MoodledemoEndpoints.LoginPath}"
            : $"{MoodledemoEndpoints.BaseUrl}{MoodledemoEndpoints.LoginPath}?{MoodledemoEndpoints.LangQueryKey}={langCode}";
        var expectedLang = langCode is null
            ? MoodledemoFixtures.English.Lang
            : MoodledemoFixtures.LoginPage.Single(row => row.Code == langCode).Lang;

        return SiteNavigation.GotoVerifiedAsync(page, url, expectedLang);
    }

    internal async Task<bool> HasLoginFormAsync() =>
        await UsernameField.CountAsync() > 0
        && await PasswordField.CountAsync() > 0
        && await (await ResolveLoginButtonAsync()).CountAsync() > 0;

    internal async Task<bool> HasLocaleMarkerAsync(string expectedLang)
    {
        var lang = await page.GetAttributeAsync("html", "lang");

        return lang == expectedLang;
    }

    internal async Task LoginAsAsync(string username, string password)
    {
        for (var attempt = 1; attempt <= LoginAttempts; attempt++)
        {
            try
            {
                if (attempt > 1)
                {
                    // the previous submit stalled or a modal overlay blocked it — start fresh
                    await GotoAsync();
                }

                await UsernameField.FillAsync(username);
                await PasswordField.FillAsync(password);
                var loginButton = await ResolveLoginButtonAsync();
                await loginButton.ClickAsync();
                await page.WaitForURLAsync(
                    url => !url.Contains(MoodledemoEndpoints.LoginPath, StringComparison.Ordinal),
                    new PageWaitForURLOptions
                    {
                        Timeout = LoginNavigationTimeoutMs,
                        WaitUntil = WaitUntilState.DOMContentLoaded
                    });

                return;
            }
            catch (TimeoutException) when (attempt < LoginAttempts)
            {
                // any step can hit a degraded variant (redirecterrordetected modal over the
                // form, dead redirect) — the filter lets the last attempt fail honestly
            }
        }
    }

    internal Task<string> ReadUsernameLabelAsync() => ReadTextAsync(_usernameLabel);

    internal Task<string> ReadPasswordLabelAsync() => ReadTextAsync(_passwordLabel);

    internal async Task<string> ReadLoginButtonTextAsync() =>
        await ReadTextAsync(await ResolveLoginButtonAsync());

    internal async Task LogoutAsync()
    {
        // the logout link lives inside a hidden user dropdown — a JS click follows the
        // redirect chain naturally (a direct GotoAsync hits net::ERR_ABORTED on the 302);
        // the href is the stable identity (localized names change per language)
        var clicked = await page.EvaluateAsync<bool>(
            @"(() => {
                const link = document.querySelector(""a[href*='/login/logout.php']"");
                if (!link) return false;
                link.click();
                return true;
            })()");
        if (!clicked)
        {
            throw new InvalidOperationException("logout link not found — user is not logged in");
        }

        await page.WaitForURLAsync(
            url => !url.Contains(MoodledemoEndpoints.LogoutPath, StringComparison.Ordinal),
            new PageWaitForURLOptions
            {
                Timeout = LoginNavigationTimeoutMs,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
        await _logoutLink.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Detached
        });
    }

    internal async Task<bool> IsLoggedInAsync() =>
        await page.Locator($"a[href*='{MoodledemoEndpoints.LogoutPath}']").CountAsync() > 0;

    internal async Task FillLoginFormAsync(string username, string password)
    {
        await UsernameField.FillAsync(username);
        await PasswordField.FillAsync(password);
    }

    internal async Task<(string Username, string Password)> ReadLoginFormValuesAsync() =>
        (await UsernameField.InputValueAsync(), await PasswordField.InputValueAsync());

    private async Task<string> ReadTextAsync(ILocator locator) =>
        TextPatterns.CollapseWhitespace(await locator.TextContentAsync());

    // The submit button's accessible name is localized — take the language from the page
    // that actually rendered (a lingering ?lang=session can serve the login page in another
    // language than the default), then the exact name from the fixture (L-LOC-3). The html
    // lang is a base tag ("zh" for ?lang=zh_cn, "pt" for pt_br) — resolve it to the fixture
    // code by code first, then by the LoginPage lang column, case-insensitively
    private async Task<ILocator> ResolveLoginButtonAsync()
    {
        var htmlLang = (await page.GetAttributeAsync("html", "lang") ?? MoodledemoFixtures.English.Code)
            .Replace('-', '_');
        var code = MoodledemoFixtures.LoginLabels
                .FirstOrDefault(row => row.Code.Equals(htmlLang, StringComparison.OrdinalIgnoreCase))?.Code
            ?? MoodledemoFixtures.LoginPage
                .Where(row => row.Lang.Equals(htmlLang, StringComparison.OrdinalIgnoreCase))
                .Select(row => row.Code)
                .FirstOrDefault(candidate => MoodledemoFixtures.LoginLabels.Any(label =>
                    label.Code.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
            ?? MoodledemoFixtures.English.Code;
        var labels = MoodledemoFixtures.LoginLabels.Single(row =>
            row.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        return page.GetByRole(AriaRole.Button, new()
        {
            Name = labels.LoginButton,
            Exact = true
        });
    }

    // Labels are not exposed as accessibility-tree elements (live dump 2026-10-22 shows only
    // their static text) — the label[for] association is the one stable handle left
    private readonly ILocator _usernameLabel = page.Locator("label[for='username']");
    private readonly ILocator _passwordLabel = page.Locator("label[for='password']");

    // live dump 2026-10-22: exactly two accessible textboxes on the login page — username
    // first, password second (the hidden guest input is not in the accessibility tree);
    // Playwright has no PasswordBox role, so order is the locale-independent identity
    private ILocator UsernameField => page.GetByRole(AriaRole.Textbox).First;

    private ILocator PasswordField => page.GetByRole(AriaRole.Textbox).Nth(1);

    private readonly ILocator _logoutLink = page.Locator($"a[href*='{MoodledemoEndpoints.LogoutPath}']").First;
}
