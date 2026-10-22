using System.Text.RegularExpressions;
using Core.Config;
using Microsoft.Playwright;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Pages;

internal sealed class LoginPage(IPage page)
{
    // the demo's post-login redirect was observed stalling past the default 15 s
    private const int LoginNavigationTimeoutMs = 30_000;
    private const int LoginAttempts = 2;
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
            if (attempt > 1)
            {
                // the previous submit stalled mid-redirect — start from a fresh login page
                await GotoAsync();
            }

            await _usernameField.FillAsync(username);
            await _passwordField.FillAsync(password);
            await _loginButton.ClickAsync();

            try
            {
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
                // the redirect did not reach DCL in time — the retry above starts over
            }
        }
    }

    internal Task<string> ReadUsernameLabelAsync() => ReadTextAsync(_usernameLabel);

    internal Task<string> ReadPasswordLabelAsync() => ReadTextAsync(_passwordLabel);

    internal Task<string> ReadLoginButtonTextAsync() => ReadTextAsync(_loginButton);

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
}
