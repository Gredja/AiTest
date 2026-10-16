using Microsoft.Playwright;

namespace Ui.Pages.GitHubStream.Repository;

internal abstract class RepositoryPageBase(IPage page)
{
    protected readonly IPage _page = page;

    internal async Task<bool> HasReadmeAsync() =>
        await _readmeLink.CountAsync() > 0;

    private readonly ILocator _readmeLink = page.GetByRole(AriaRole.Link, new() { Name = "README.md, (File)" });
}
