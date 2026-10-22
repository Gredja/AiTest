using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class LanguageSwitcherUiTests : MoodledemoTestBase
{
    [Test]
    [Category("Smoke")]
    [Description("5.1 Language menu lists every language except the active one, marked aria-current")]
    public async Task LanguageMenu_ListsEveryLanguageExceptCurrent_MarkedAriaCurrent()
    {
        await Browser.HomePage.GotoAsync();
        await Browser.HomePage.OpenLanguageMenuAsync();

        var linkCount = await Browser.HomePage.LanguageLinks.CountAsync();
        linkCount.Should().Be(
            MoodledemoFixtures.Home.Count - 1,
            "the active language is not switchable — only the other 147 codes are links");

        var englishLinks = await Browser.HomePage.CountLanguageLinksAsync("en");
        englishLinks.Should().Be(1, "plain en must be offered while en_us is active");

        var variantLinks = await Browser.HomePage.CountLanguageLinksAsync("en_us");
        variantLinks.Should().Be(0, "the active en_us must not be a switchable link");

        var currentItems = await Browser.HomePage.CurrentLanguageItem.CountAsync();
        currentItems.Should().Be(1, "the active language must appear as the aria-current marker");

        var currentItemText = await Browser.HomePage.CurrentLanguageItem.TextContentAsync();
        currentItemText.Should().Contain("(en_us)", "the marker must carry the active language code");

        var currentText = await Browser.HomePage.CurrentLanguageTextAsync();
        currentText.Should().Contain("(en_us)", "the toggle must show the active language code");
    }

    [Test]
    [Category("Smoke")]
    [Description("5.2 Clicking a language navigates with ?lang and re-renders the locale")]
    public async Task LanguageMenu_Click_SwitchesLocale()
    {
        await Browser.HomePage.GotoAsync();
        await Browser.HomePage.OpenLanguageMenuAsync();
        await Browser.HomePage.ClickLanguageAsync("de");

        var lang = await PageLocaleReader.ReadLangAsync(Browser.Page);
        lang.Should().Be("de", "the clicked language must be applied to the page");

        var currentText = await Browser.HomePage.CurrentLanguageTextAsync();
        currentText.Should().Contain("(de)", "the toggle must follow the switched language");
    }

    [Test]
    [Category("Smoke")]
    [Description("5.3 en_us is distinguishable from en: html lang is identical, the toggle keeps the variant")]
    public async Task EnUs_DistinguishableFromEn_OnToggle()
    {
        await Browser.HomePage.GotoAsync();

        // en-US Accept-Language → en_us, but the html lang normalizes to the parent "en"
        var defaultLang = await PageLocaleReader.ReadLangAsync(Browser.Page);
        defaultLang.Should().Be("en", "the html lang of the en_us default normalizes to en");

        var variantText = await Browser.HomePage.CurrentLanguageTextAsync();
        variantText.Should().Contain("(en_us)", "the toggle keeps the regional variant code");

        await Browser.HomePage.GotoAsync("en");

        var englishLang = await PageLocaleReader.ReadLangAsync(Browser.Page);
        englishLang.Should().Be("en", "plain en renders with html lang=en");

        var englishText = await Browser.HomePage.CurrentLanguageTextAsync();
        englishText.Should().Contain("(en)", "the toggle must switch to the plain en code");
        englishText.Should().NotBe(variantText, "en and en_us must read as different selections");
    }
}
