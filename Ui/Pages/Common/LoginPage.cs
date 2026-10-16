using Core.Config;
using Microsoft.Playwright;

namespace Ui.Pages.Common;

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

    private readonly ILocator _usernameTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Username or email address" });
    private readonly ILocator _passwordTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "Password" });
    private readonly ILocator _signInButton = page.GetByRole(AriaRole.Button, new() { Name = "Sign in", Exact = true });
}
