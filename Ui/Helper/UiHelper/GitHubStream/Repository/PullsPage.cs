using Core.Config;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Repository;

internal sealed class PullsPage
{
    private readonly IPage _page;

    internal PullsPage(IPage page) => _page = page;

    internal async Task OpenAsync()
    {
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/pulls");
    }

    internal async Task<bool> HasEmptyStateAsync() =>
        await _page.GetByRole(AriaRole.Heading, new() { Name = "No pull requests matched your search" }).CountAsync() > 0;

    // Tab counter renders as "Open (0)" (anonymous) or "Open 0" (authenticated) — verified 2026-10-13
    internal async Task<int> GetOpenCountAsync() => await GetTabCountAsync("Open");

    internal async Task<int> GetClosedCountAsync() => await GetTabCountAsync("Closed");

    private async Task<int> GetTabCountAsync(string tabName)
    {
        var text = await _page.GetByRole(AriaRole.Link)
            .Filter(new() { HasTextString = tabName })
            .First.TextContentAsync();
        var match = System.Text.RegularExpressions.Regex.Match(text ?? string.Empty, $@"{tabName}\D*(\d+)");

        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
    }
}
