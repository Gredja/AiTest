using Core.Config;
using Microsoft.Playwright;

namespace Ui.Github.Pages.GitHubStream.Repository;

internal sealed class RepositoryPage(IPage page) : RepositoryPageBase(page)
{
    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}");

    internal async Task<string> GetDefaultBranchAsync() =>
        (await _branchButton.TextContentAsync())?.Trim() ?? string.Empty;

    internal async Task<int> GetDescriptionOccurrencesAsync(string description) =>
        await _page.GetByText(description).CountAsync();

    private readonly ILocator _branchButton = page.GetByRole(AriaRole.Button, new() { Name = "main branch" });
}
