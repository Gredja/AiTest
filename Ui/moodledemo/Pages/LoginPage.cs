using Core.Config;
using Microsoft.Playwright;

namespace Ui.Moodledemo.Pages;

internal sealed class LoginPage(IPage page)
{
    internal Task GotoAsync(string? langCode = null)
    {
        var url = langCode is null
            ? $"{MoodledemoEndpoints.BaseUrl}{MoodledemoEndpoints.LoginPath}"
            : $"{MoodledemoEndpoints.BaseUrl}{MoodledemoEndpoints.LoginPath}?{MoodledemoEndpoints.LangQueryKey}={langCode}";

        return page.GotoAsync(url);
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

    private readonly ILocator _usernameField = page.Locator("input[name='username']");
    private readonly ILocator _passwordField = page.Locator("input[name='password']");
    private readonly ILocator _loginButton = page.Locator("#loginbtn");
}
