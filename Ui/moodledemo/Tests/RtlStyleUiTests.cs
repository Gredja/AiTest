using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class RtlStyleUiTests : MoodledemoTestBase
{
    private static IEnumerable<TestCaseData> RtlStyleCases()
    {
        foreach (var expectation in MoodledemoFixtures.Home.Where(row => row.Dir == "rtl"))
        {
            yield return new TestCaseData(expectation)
                .SetName($"RtlLocales_BodyAndNavMirrored({expectation.Code})");
        }
    }

    [Test]
    [Category("Regression")]
    [Description("C.1 RTL locales mirror body and nav computed direction")]
    [TestCaseSource(nameof(RtlStyleCases))]
    public async Task RtlLocales_BodyAndNavMirrored(LocaleExpectation expectation)
    {
        await Browser.HomePage.GotoAsync(expectation.Code);

        var bodyDirection = await Browser.HomePage.ReadBodyDirectionAsync();
        bodyDirection.Should().Be(
            expectation.Dir,
            "the computed body direction must mirror the layout for {0}", expectation.Code);

        var navDirection = await Browser.HomePage.ReadNavDirectionAsync();
        navDirection.Should().Be(
            expectation.Dir,
            "the computed nav direction must mirror the layout for {0}", expectation.Code);
    }
}
