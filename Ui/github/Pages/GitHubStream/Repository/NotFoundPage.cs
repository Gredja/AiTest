using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;
using Ui.Github.Pages.Common;

namespace Ui.Github.Pages.GitHubStream.Repository;

internal sealed class NotFoundPage(IPage page)
{
    private static readonly TimeSpan _messageTimeout =
        TimeSpan.FromMilliseconds(UiTimeout.UiTimeoutMs);

    private readonly IPage _page = page;

    internal async Task OpenAsync() =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{GitHubEndpoints.NonExistentUiRepoPath}");

    internal async Task<WaitResult<bool>> WaitFor404MessageAsync() =>
        await WaitHelper.WaitUntilAsync(
            async () => await _notFoundImage.CountAsync() > 0,
            hasMessage => hasMessage,
            _messageTimeout);

    private readonly ILocator _notFoundImage = page.GetByRole(AriaRole.Img,
        new() { Name = "404 “This is not the web page you are looking for”" });
}
