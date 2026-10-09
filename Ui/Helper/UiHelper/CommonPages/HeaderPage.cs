using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.CommonPages;

internal sealed class HeaderPage
{
    private readonly IPage _page;

    internal HeaderPage(IPage page) => _page = page;

    internal async Task<string> GetSignedInUsernameAsync()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Open user navigation menu" }).ClickAsync();
        var heading = _page.GetByRole(AriaRole.Dialog, new() { Name = "User navigation" })
            .GetByRole(AriaRole.Heading);
        await heading.WaitForAsync();

        return (await heading.TextContentAsync())?.Trim() ?? string.Empty;
    }
}
