using Core.Config;
using Microsoft.Playwright;

namespace Ui.Helpers;

internal static class SiteNavigationHelper
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

    // The external demo occasionally serves degraded responses (timeout, a foreign page,
    // the default locale for a ?lang request); navigate until the requested locale arrives
    // so tests assert persistence, not transient noise. The last failed attempt is left
    // to the test assertions to report.
    internal static async Task GotoVerifiedAsync(IPage page, string url, string expectedLang)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded
                });
            }
            catch (TimeoutException) when (attempt < MaxAttempts)
            {
                await Task.Delay(_retryDelay);
                continue;
            }

            var title = await page.TitleAsync();
            var lang = await page.GetAttributeAsync("html", "lang");
            if (title.Contains(MoodledemoEndpoints.SiteTitleMarker, StringComparison.Ordinal)
                && lang == expectedLang)
            {
                return;
            }

            await Task.Delay(_retryDelay);
        }
    }
}
