using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class RtlLayoutUiTests : MoodledemoTestBase
{
    private static IEnumerable<TestCaseData> RtlCases()
    {
        foreach (var expectation in MoodledemoFixtures.Home.Where(row => row.Dir == "rtl"))
        {
            yield return new TestCaseData(expectation)
                .SetName($"Home_RtlLayout_RendersMirrored({expectation.Code})");
        }
    }

    [Test]
    [Category("Regression")]
    [Description("7.1 RTL locales render mirrored: html dir, computed body direction, fixture title")]
    [TestCaseSource(nameof(RtlCases))]
    public async Task Home_RtlLayout_RendersMirrored(LocaleExpectation expectation)
    {
        await Browser.HomePage.GotoAsync(expectation.Code);

        var lang = await PageLocaleReader.ReadLangAsync(Browser.Page);
        lang.Should().Be(expectation.Lang, "the html lang must match the fixture for {0}", expectation.Code);

        var dir = await PageLocaleReader.ReadDirAsync(Browser.Page);
        dir.Should().Be(expectation.Dir, "the html dir must match the fixture for {0}", expectation.Code);

        var bodyDirection = await PageLocaleReader.ReadBodyDirectionAsync(Browser.Page);
        bodyDirection.Should().Be(
            expectation.Dir,
            "the computed body direction must mirror the layout for {0}", expectation.Code);

        var title = await Browser.Page.TitleAsync();
        title.Should().Be(expectation.Title, "the title must match the fixture for {0}", expectation.Code);
    }
}
