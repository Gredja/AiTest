using Core.Config;
using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class LanguagePersistenceUiTests : MoodledemoTestBase
{
    private const string RussianCode = "ru";

    [Test]
    [Category("Regression")]
    [Description("6.1 Selected language survives a navigation to a URL without a lang parameter")]
    public async Task SelectedLanguage_SurvivesNavigationWithoutLangParameter()
    {
        await Browser.HomePage.GotoAsync(RussianCode);
        await Browser.HomePage.OpenCalendarAsync();

        var lang = await PageLocaleReader.ReadLangAsync(Browser.Page);
        lang.Should().Be(RussianCode, "the session language must persist on a lang-less URL");
    }

    [Test]
    [Category("Regression")]
    [Description("6.2 A language chosen before login does NOT survive authentication (demo accounts reset to English)")]
    public async Task PreLoginLanguage_ResetsToEnglishAfterLogin()
    {
        await Browser.HomePage.GotoAsync(RussianCode);
        await Browser.LoginPage.GotoAsync();
        await Browser.LoginPage.LoginAsAsync(
            TestConfig.MoodleDemoStudentUsername,
            TestConfig.MoodleDemoPassword);

        var lang = await PageLocaleReader.ReadLangAsync(Browser.Page);
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
        await Browser.HomePage.GotoAsync(RussianCode);
        var selectedLang = await PageLocaleReader.ReadLangAsync(Browser.Page);
        selectedLang.Should().Be(RussianCode);

        await Browser.HomePage.GotoMyDashboardAsync(RussianCode);
        var lang = await PageLocaleReader.ReadLangAsync(Browser.Page);
        lang.Should().Be(RussianCode, "the language chosen while logged in must persist on a lang-less URL");
    }

    [Test]
    [Category("Regression")]
    [Description("6.4 Switching to en_us resets the language back to English")]
    public async Task SwitchingToEnUs_ResetsLanguage()
    {
        await Browser.HomePage.GotoAsync(RussianCode);
        var selectedLang = await PageLocaleReader.ReadLangAsync(Browser.Page);
        selectedLang.Should().Be(RussianCode);

        var englishUs = MoodledemoFixtures.Home.Single(row => row.Code == "en_us");
        await Browser.HomePage.GotoAsync(englishUs.Code);

        var resetLang = await PageLocaleReader.ReadLangAsync(Browser.Page);
        resetLang.Should().Be(englishUs.Lang, "en_us must reset the session language to English");

        var resetTitle = await Browser.Page.TitleAsync();
        resetTitle.Should().Be(englishUs.Title, "the page must render the English title after the reset");

        var currentText = await Browser.HomePage.CurrentLanguageTextAsync();
        currentText.Should().Contain("(en_us)", "the toggle must show the chosen regional variant");
    }
}
