using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Github.Pages.Common;

internal sealed class ProfilePage(IPage page)
{
    private static readonly TimeSpan _headingTimeout =
        TimeSpan.FromMilliseconds(UiTimeout.UiTimeoutMs);

    internal async Task<WaitResult<bool>> WaitForUsernameHeadingAsync() =>
        await WaitHelper.WaitUntilAsync(
            async () => await _usernameHeading.CountAsync() > 0,
            isPresent => isPresent,
            _headingTimeout);

    private readonly ILocator _usernameHeading = page.GetByRole(AriaRole.Heading, new() { Name = TestConfig.UiUsername, Level = 1 });
}
