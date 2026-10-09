using Core.Config;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.CommonPages;

internal sealed class LoginPage
{
    private readonly IPage _page;

    internal LoginPage(IPage page) => _page = page;

    internal async Task OpenAsync()
    {
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/login");
    }

    internal async Task SignInAsync(string login, string password)
    {
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Username or email address" }).FillAsync(login);
        await _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync(password);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign in", Exact = true }).ClickAsync();
    }
}
