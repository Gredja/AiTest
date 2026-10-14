using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.CommonPages;

internal sealed class HeaderPage
{
    private static readonly TimeSpan MenuOpenTimeout = TimeSpan.FromSeconds(5);

    // The menu renders ~300ms AFTER the click (measured 2026-10-14), and while it is open the
    // trigger loses its accessible name — a retry inside that window cannot find the button and
    // burns the whole 15s action timeout. The retry interval must stay well above the open latency.
    private static readonly TimeSpan MenuOpenRetryInterval = TimeSpan.FromSeconds(1);

    private readonly IPage _page;

    internal HeaderPage(IPage page) => _page = page;

    internal async Task<string> GetSignedInUsernameAsync()
    {
        var dialog = _page.GetByRole(AriaRole.Dialog, new() { Name = "User navigation" });
        await OpenUserMenuAsync(dialog);

        var heading = dialog.GetByRole(AriaRole.Heading);
        await heading.WaitForAsync();

        return (await heading.TextContentAsync())?.Trim() ?? string.Empty;
    }

    // The item is a link (href /logout) whose accessible name comes from aria-labelledby — verified 2026-10-14
    internal async Task SignOutAsync()
    {
        var dialog = _page.GetByRole(AriaRole.Dialog, new() { Name = "User navigation" });
        await OpenUserMenuAsync(dialog);

        await _page.GetByRole(AriaRole.Link, new() { Name = "Sign out", Exact = true }).ClickAsync();

        // /logout is an account chooser, not the sign-out itself: the per-account submit carries the
        // username in its aria-label, while the sibling "Sign out from all accounts" sets destroy_all
        // and would kill the shared storageState session — it must never be clicked
        await _page.GetByRole(AriaRole.Button, new() { Name = $"Sign out of {TestConfig.UiUsername}", Exact = true })
            .ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Sign in", Exact = true }).First.WaitForAsync();
    }

    internal async Task<bool> IsSignedOutAsync() =>
        await _page.GetByRole(AriaRole.Link, new() { Name = "Sign in", Exact = true }).CountAsync() > 0;

    // The click can be swallowed while the header is still wiring up — flaky right after the login
    // redirect (2026-10-14: menu never opened, dialog absent after 15s). The open dialog is the
    // post-condition, so click again until it is up; the state check runs BEFORE every click, so a
    // menu that merely renders late is never toggled shut.
    private async Task OpenUserMenuAsync(ILocator dialog)
    {
        var menuButton = _page.GetByRole(AriaRole.Button, new() { Name = "Open user navigation menu" });

        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                if (await dialog.CountAsync() > 0)
                {
                    return true;
                }

                await menuButton.ClickAsync();
                return false;
            },
            isOpen => isOpen,
            MenuOpenTimeout,
            MenuOpenRetryInterval);
    }
}
