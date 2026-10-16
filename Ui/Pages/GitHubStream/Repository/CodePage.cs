using Core.Config;
using Microsoft.Playwright;

namespace Ui.Pages.GitHubStream.Repository;

internal sealed class CodePage(IPage page) : RepositoryPageBase(page)
{
    internal async Task OpenAsync() => await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/tree/main");
}
