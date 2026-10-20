using Microsoft.Playwright;
using Ui.Moodledemo.Pages;

namespace Ui.Moodledemo.Tests.Support;

internal sealed class MoodledemoBrowser
{
    internal MoodledemoBrowser(IPage page)
    {
        Page = page;
        LoginPage = new LoginPage(page);
    }

    internal IPage Page { get; }
    internal LoginPage LoginPage { get; }
}
