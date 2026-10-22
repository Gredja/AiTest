using System.Text.RegularExpressions;
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
        await _usernameField.CountAsync() > 0
        && await _passwordField.CountAsync() > 0
        && await _loginButton.CountAsync() > 0;

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

                await _usernameField.FillAsync(username);
                await _passwordField.FillAsync(password);
                await _loginButton.ClickAsync();
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

    internal Task<string> ReadLoginButtonTextAsync() => ReadTextAsync(_loginButton);

    internal async Task LogoutAsync()
    {
        // the logout link lives inside a hidden user dropdown — a JS click follows the
        // redirect chain naturally (a direct GotoAsync hits net::ERR_ABORTED on the 302)
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
        await _usernameField.FillAsync(username);
        await _passwordField.FillAsync(password);
    }

    internal async Task<(string Username, string Password)> ReadLoginFormValuesAsync() =>
        (await _usernameField.InputValueAsync(), await _passwordField.InputValueAsync());

    private static string CollapseWhitespace(string? text) =>
        Regex.Replace(text ?? string.Empty, @"\s+", " ").Trim();

    private async Task<string> ReadTextAsync(ILocator locator) =>
        CollapseWhitespace(await locator.TextContentAsync());

    // :visible — Moodle may render a hidden guest-login input with the same name
    private readonly ILocator _usernameField = page.Locator("input[name='username']:visible");
    private readonly ILocator _passwordField = page.Locator("input[name='password']:visible");
    private readonly ILocator _loginButton = page.Locator("#loginbtn");
    private readonly ILocator _usernameLabel = page.Locator("label[for='username']");
    private readonly ILocator _passwordLabel = page.Locator("label[for='password']");
    private readonly ILocator _logoutLink = page.Locator($"a[href*='{MoodledemoEndpoints.LogoutPath}']").First;
}
