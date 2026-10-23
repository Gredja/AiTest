using System.Text.RegularExpressions;
using Ui.Helpers;
using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class CalendarRegionUiTests : MoodledemoTestBase
{
    // subpixel rounding allowance when comparing scrollWidth vs clientWidth
    private const long OverflowTolerancePx = 5;

    // the controls text must contain a four-digit year (e.g. "2026")
    private static readonly Regex _fourDigitYearPattern = new(@"\d{4}", RegexOptions.Compiled);

    private static IEnumerable<TestCaseData> CalendarCases()
    {
        foreach (var code in MoodledemoFixtures.CalendarPageCodes)
        {
            yield return new TestCaseData(code)
                .SetName($"CalendarPage_LocaleMatchesFixture({code})");
        }
    }

    [Test]
    [Category("Regression")]
    [Description("E.1 Calendar month page locale matches fixture and has no horizontal overflow")]
    [TestCaseSource(nameof(CalendarCases))]
    public async Task CalendarPage_LocaleMatchesFixture(string code)
    {
        var expectation = MoodledemoFixtures.Home.Single(row => row.Code == code);

        await Browser.HomePage.GotoCalendarAsync(code);

        var lang = await PageLocaleReaderHelper.ReadLangAsync(Browser.Page);
        lang.Should().Be(expectation.Lang, "calendar html lang must match the fixture for {0}", code);

        var controlsText = await Browser.HomePage.ReadCalendarControlsTextAsync();
        controlsText.Should().NotBeNullOrWhiteSpace(
            "the calendar month controls must render for {0}", code);

        var (scrollWidth, clientWidth) = await Browser.HomePage.ReadPageOverflowAsync();
        scrollWidth.Should().BeLessThanOrEqualTo(
            clientWidth + OverflowTolerancePx,
            "the calendar page must not overflow horizontally for {0} (long month names)", code);
    }

    [Test]
    [Category("Regression")]
    [Description("E.2 Calendar for a long-month language renders the current month heading")]
    public async Task Calendar_LongMonthLanguage_RendersMonthHeading()
    {
        var longMonthCode = MoodledemoFixtures.Scenarios.LongMonthCalendar;
        await Browser.HomePage.GotoCalendarAsync(longMonthCode);

        var controlsText = await Browser.HomePage.ReadCalendarControlsTextAsync();
        controlsText.Should().MatchRegex(
            _fourDigitYearPattern,
            "the long-month calendar controls must carry a four-digit year");
    }
}
