using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Github.Pages.Common;

internal sealed class AccountSettingsPage(IPage page)
{
    private static readonly TimeSpan _headingTimeout =
        TimeSpan.FromMilliseconds(UiTimeout.UiTimeoutMs);

    internal async Task<WaitResult<bool>> WaitForAccountHeadingAsync() =>
        await WaitHelper.WaitUntilAsync(
            async () => await _accountHeading.CountAsync() > 0,
            isPresent => isPresent,
            _headingTimeout);

    // display name in the heading may differ from the login — match the "(login) settings" part
    private readonly ILocator _accountHeading = page.GetByRole(AriaRole.Heading, new() { Name = $"({TestConfig.UiUsername}) settings", Level = 1 });
}
