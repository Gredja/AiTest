using Core.Config;
using Microsoft.Playwright;

namespace Ui.Helper.UiHelper.GitHubStream.Repository;

internal sealed class RepositoryPage
{
    private readonly IPage _page;

    internal RepositoryPage(IPage page) => _page = page;

    internal async Task OpenAsync()
    {
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}");
    }

    internal async Task<string> GetDefaultBranchAsync()
    {
        var branchButton = _page.GetByRole(AriaRole.Button, new() { Name = "main branch" });
        return (await branchButton.TextContentAsync())?.Trim() ?? string.Empty;
    }

    internal async Task<bool> HasReadmeAsync() =>
        await _page.GetByRole(AriaRole.Link, new() { Name = "README.md, (File)" }).CountAsync() > 0;
}
