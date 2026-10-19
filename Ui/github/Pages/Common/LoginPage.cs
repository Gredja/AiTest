using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Github.Pages.Common;

internal sealed class LoginPage(IPage page)
{
    private readonly IPage _page = page;

    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/login");

    internal async Task SignInAsync(string login, string password)
    {
        await _usernameTextbox.FillAsync(login);
        await _passwordTextbox.FillAsync(password);
        await _signInButton.ClickAsync();
        await _page.WaitForURLAsync(url => !url.Contains("/login"));
    }

    internal async Task<WaitResult<bool>> WaitForLoginRedirectAsync() =>
        await WaitHelper.WaitUntilAsync(
            async () => _page.Url.Contains("/login") && await _signInHeading.CountAsync() > 0,
            isLogin => isLogin);

    internal async Task<bool> HasSignInLinkAsync() =>
        await _signInLink.CountAsync() > 0;

    private readonly ILocator _usernameTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Username or email address" });
    private readonly ILocator _passwordTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Password" });
    private readonly ILocator _signInButton = page.GetByRole(AriaRole.Button, new() { Name = "Sign in", Exact = true });
    private readonly ILocator _signInHeading = page.GetByRole(AriaRole.Heading, new() { Name = "Sign in to GitHub" });
    private readonly ILocator _signInLink = page.GetByRole(AriaRole.Link, new() { Name = "Sign in", Exact = true });
}
