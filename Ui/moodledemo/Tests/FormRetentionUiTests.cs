using FluentAssertions;
using Ui.Helpers;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[AllureNUnit]
[Category("MoodleDemo")]
public class FormRetentionUiTests : MoodledemoTestBase
{
    [Test]
    [Category("Regression")]
    [Description("F.1 Language switch via ?lang navigates (full reload) — the form is reset, which is expected for a multi-page app")]
    public async Task LanguageSwitch_ResetsLoginForm()
    {
        var switchTarget = MoodledemoFixtures.Scenarios.SwitchTarget;

        await Browser.LoginPage.GotoAsync();
        await Browser.LoginPage.FillLoginFormAsync("typed_username", "typed_password");

        await Browser.LoginPage.GotoAsync(switchTarget);

        var (username, password) = await Browser.LoginPage.ReadLoginFormValuesAsync();
        username.Should().BeEmpty(
            "?lang= is a full page navigation — Moodle reloads and the form is not retained (expected for a multi-page app, not a bug)");
        password.Should().BeEmpty(
            "?lang= is a full page navigation — Moodle reloads and the form is not retained (expected for a multi-page app, not a bug)");

        var lang = await PageLocaleReaderHelper.ReadLangAsync(Browser.Page);
        lang.Should().Be(switchTarget, "the login page must still honor the language switch");
    }

    [Test]
    [Category("Regression")]
    [Description("F.2 Login form is still usable after a language switch")]
    public async Task LoginForm_StillUsableAfterLanguageSwitch()
    {
        var switchTarget = MoodledemoFixtures.Scenarios.SwitchTarget;

        await Browser.LoginPage.GotoAsync();
        await Browser.LoginPage.GotoAsync(switchTarget);

        (await Browser.LoginPage.HasLoginFormAsync()).Should().BeTrue(
            $"the login form must still be present after switching to {switchTarget}");

        var button = await Browser.LoginPage.ReadLoginButtonTextAsync();
        button.Should().NotBeNullOrWhiteSpace("the login button must still render after the switch");
    }
}
