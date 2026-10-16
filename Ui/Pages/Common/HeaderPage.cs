using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Pages.Common;

internal sealed class HeaderPage(IPage page)
{
    private static readonly TimeSpan _menuOpenTimeout = TimeSpan.FromSeconds(5);

    // The menu renders ~300ms AFTER the click (measured 2026-10-14), and while it is open the
    // trigger loses its accessible name — a retry inside that window cannot find the button and
    // burns the whole 15s action timeout. The retry interval must stay well above the open latency.
    private static readonly TimeSpan _menuOpenRetryInterval = TimeSpan.FromSeconds(1);

    private readonly IPage _page = page;

    internal async Task<string> GetSignedInUsernameAsync()
    {
        await OpenUserMenuAsync(_userNavigationDialog);

        await _userNavigationHeading.WaitForAsync();

        return (await _userNavigationHeading.TextContentAsync())?.Trim() ?? string.Empty;
    }

    internal async Task SignOutAsync()
    {
        await OpenUserMenuAsync(_userNavigationDialog);

        await _signOutLink.ClickAsync();

        // /logout is an account chooser, not the sign-out itself: the per-account submit carries the
        // username in its aria-label, while the sibling "Sign out from all accounts" sets destroy_all
        // and would kill the shared storageState session — it must never be clicked
        await _accountSignOutButton.ClickAsync();
        await _firstSignInLink.WaitForAsync();
    }

    internal async Task<bool> IsSignedOutAsync() =>
        await _signInLink.CountAsync() > 0;

    // The click can be swallowed while the header is still wiring up — flaky right after the login
    // redirect (2026-10-14: menu never opened, dialog absent after 15s). The open dialog is the
    // post-condition, so click again until it is up; the state check runs BEFORE every click, so a
    // menu that merely renders late is never toggled shut.
    private async Task OpenUserMenuAsync(ILocator dialog)
    {
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                if (await dialog.CountAsync() > 0)
                {
                    return true;
                }

                await _userMenuButton.ClickAsync();

                return false;
            },
            isOpen => isOpen,
            timeout: _menuOpenTimeout,
            interval: _menuOpenRetryInterval);
    }

    private readonly ILocator _userNavigationDialog = page.GetByRole(AriaRole.Dialog, new() { Name = "User navigation" });
    private readonly ILocator _userNavigationHeading = page.GetByRole(AriaRole.Dialog, new() { Name = "User navigation" }).GetByRole(AriaRole.Heading);
    private readonly ILocator _signOutLink = page.GetByRole(AriaRole.Link, new() { Name = "Sign out", Exact = true });
    private readonly ILocator _accountSignOutButton = page.GetByRole(AriaRole.Button, new() { Name = $"Sign out of {TestConfig.UiUsername}", Exact = true });
    private readonly ILocator _signInLink = page.GetByRole(AriaRole.Link, new() { Name = "Sign in", Exact = true });
    private readonly ILocator _firstSignInLink = page.GetByRole(AriaRole.Link, new() { Name = "Sign in", Exact = true }).First;
    private readonly ILocator _userMenuButton = page.GetByRole(AriaRole.Button, new() { Name = "Open user navigation menu" });
}
