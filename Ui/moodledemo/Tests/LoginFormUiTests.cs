using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class LoginFormUiTests : MoodledemoTestBase
{
    [Test]
    [Category("Smoke")]
    [Description("5.4 Browser: login form renders with role-based locators")]
    public async Task LoginForm_RendersWithStableSelectors()
    {
        await Browser.LoginPage.GotoAsync();

        var hasForm = await Browser.LoginPage.HasLoginFormAsync();
        hasForm.Should().BeTrue("the login form must expose username, password and the submit button");

        var hasLocale = await Browser.LoginPage.HasLocaleMarkerAsync("en");
        hasLocale.Should().BeTrue("the default session must render as lang=en");
    }
}
