using Core.Config;
using Ui.Helpers;
using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class LanguagePersistenceUiTests : MoodledemoTestBase
{
    // login flows in this fixture type the demo password — never record DOM snapshots
    // (screenshots on failure stay on; only the trace zip is skipped)
    protected override bool IsCredentialEntryTest => true;

    private static string SessionLanguage => MoodledemoFixtures.Scenarios.SessionLanguage;

    [Test]
    [Category("Regression")]
    [Description("6.1 Selected language survives a navigation to a URL without a lang parameter")]
    public async Task SelectedLanguage_SurvivesNavigationWithoutLangParameter()
    {
        await Browser.HomePage.GotoAsync(SessionLanguage);
        await Browser.HomePage.OpenCalendarAsync();

        var lang = await PageLocaleReaderHelper.ReadLangAsync(Browser.Page);
        lang.Should().Be(SessionLanguage, "the session language must persist on a lang-less URL");
    }

    [Test]
    [Category("Regression")]
    [Description("6.2 A language chosen before login does NOT survive authentication (demo accounts reset to English)")]
    public async Task PreLoginLanguage_ResetsToEnglishAfterLogin()
    {
        await Browser.HomePage.GotoAsync(SessionLanguage);
        await Browser.LoginPage.GotoAsync();
        await Browser.LoginPage.LoginAsAsync(
            TestConfig.MoodleDemoStudentUsername,
            TestConfig.MoodleDemoPassword);

        var lang = await PageLocaleReaderHelper.ReadLangAsync(Browser.Page);
        lang.Should().Be(
            MoodledemoFixtures.English.Lang,
            "observed 2026-10-22: authentication re-applies the account/site language over the session choice");
    }

    [Test]
    [Category("Regression")]
    [Description("6.3 A language chosen after login survives a navigation without a lang parameter")]
    public async Task LanguageChosenAfterLogin_SurvivesNavigation()
    {
        await Browser.LoginPage.GotoAsync();
        await Browser.LoginPage.LoginAsAsync(
            TestConfig.MoodleDemoStudentUsername,
            TestConfig.MoodleDemoPassword);

        // the header hides the switcher for authenticated users — the ?lang URL is the way in
        await Browser.HomePage.GotoAsync(SessionLanguage);
        var selectedLang = await PageLocaleReaderHelper.ReadLangAsync(Browser.Page);
        selectedLang.Should().Be(SessionLanguage);

        await Browser.HomePage.GotoMyDashboardAsync(SessionLanguage);
        var lang = await PageLocaleReaderHelper.ReadLangAsync(Browser.Page);
        lang.Should().Be(SessionLanguage, "the language chosen while logged in must persist on a lang-less URL");
    }

    [Test]
    [Category("Regression")]
    [Description("6.4 Switching to en_us resets the language back to English")]
    public async Task SwitchingToEnUs_ResetsLanguage()
    {
        await Browser.HomePage.GotoAsync(SessionLanguage);
        var selectedLang = await PageLocaleReaderHelper.ReadLangAsync(Browser.Page);
        selectedLang.Should().Be(SessionLanguage);

        var activeDefault = MoodledemoFixtures.Scenarios.ActiveDefault;
        var englishUs = MoodledemoFixtures.Home.Single(row => row.Code == activeDefault);
        await Browser.HomePage.GotoAsync(englishUs.Code);

        var resetLang = await PageLocaleReaderHelper.ReadLangAsync(Browser.Page);
        resetLang.Should().Be(englishUs.Lang, "en_us must reset the session language to English");

        var resetTitle = await Browser.Page.TitleAsync();
        resetTitle.Should().Be(englishUs.Title, "the page must render the English title after the reset");

        var currentText = await Browser.HomePage.CurrentLanguageTextAsync();
        currentText.Should().Contain($"({activeDefault})", "the toggle must show the chosen regional variant");
    }
}
