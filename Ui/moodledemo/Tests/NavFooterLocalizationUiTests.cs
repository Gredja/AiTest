using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class NavFooterLocalizationUiTests : MoodledemoTestBase
{
    private static IEnumerable<TestCaseData> NavLabelCases()
    {
        foreach (var labels in MoodledemoFixtures.NavLabels)
        {
            yield return new TestCaseData(labels)
                .SetName($"HomeNavLabels_MatchFixture({labels.Code})");
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("B.1 Home top navigation labels match the localized fixture")]
    [TestCaseSource(nameof(NavLabelCases))]
    public async Task HomeNavLabels_MatchFixture(NavLabelExpectation labels)
    {
        await Browser.HomePage.GotoAsync(labels.Code);

        var navLabels = await Browser.HomePage.ReadNavLabelsAsync();

        navLabels.Should().Contain(labels.Home, "home nav label must be localized for {0}", labels.Code);
        navLabels.Should().Contain(labels.Calendar, "calendar nav label must be localized for {0}", labels.Code);
        navLabels.Should().Contain(labels.More, "more nav label must be localized for {0}", labels.Code);

        if (labels.HomeIsEnglishGap)
        {
            navLabels.Should().Contain(
                "Home",
                "ja home nav is a documented English fallback (known gap, not a bug)");
        }
    }

    [Test]
    [Category("Regression")]
    [Description("B.2 Guest footer contains a login link")]
    public async Task GuestFooter_ContainsLoginLink()
    {
        await Browser.HomePage.GotoAsync();

        var loginLinks = Browser.Page.Locator($"footer a[href*='{Core.Config.MoodledemoEndpoints.LoginPath}']");

        (await loginLinks.CountAsync()).Should().BeGreaterThan(
            0,
            "the guest footer must offer a way to log in");
    }
}
