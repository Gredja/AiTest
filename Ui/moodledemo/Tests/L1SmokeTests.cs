using System.Net;
using Core.Config;
using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class L1SmokeTests
{
    private static IEnumerable<TestCaseData> HomeCases()
    {
        foreach (var expectation in MoodledemoFixtures.Home)
        {
            yield return new TestCaseData(expectation)
                .SetName($"Home_Lang_MatchesFixture({expectation.Code})");
        }
    }

    private static IEnumerable<TestCaseData> LoginPageCases()
    {
        foreach (var expectation in MoodledemoFixtures.LoginPage)
        {
            yield return new TestCaseData(expectation)
                .SetName($"LoginPage_Lang_PersistsLocale({expectation.Code})");
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("1.1 GET / default: 200, lang=en, dir=ltr, English title")]
    public async Task Home_Default_IsEnglishLtr()
    {
        var english = MoodledemoFixtures.English;
        var (status, body) = await MoodledemoHttp.GetAsync(MoodledemoEndpoints.HomePath);

        status.Should().Be(HttpStatusCode.OK);
        var (lang, dir) = HtmlLocaleReader.ReadLocale(body);
        lang.Should().Be(english.Lang);
        dir.Should().Be(english.Dir);
        HtmlLocaleReader.ReadTitle(body).Should().Be(english.Title);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.2 GET /?lang={code}: locale attributes and title match the fixture")]
    [TestCaseSource(nameof(HomeCases))]
    public async Task Home_Lang_MatchesFixture(LocaleExpectation expectation)
    {
        var (status, body) = await MoodledemoHttp.GetAsync(MoodledemoHttp.HomeWithLang(expectation.Code));

        status.Should().Be(HttpStatusCode.OK);
        var (lang, dir) = HtmlLocaleReader.ReadLocale(body);
        lang.Should().Be(expectation.Lang);
        dir.Should().Be(expectation.Dir);
        HtmlLocaleReader.ReadTitle(body).Should().Be(expectation.Title);
    }

    [Test]
    [Category("Smoke")]
    [Description("2.1 GET /login/index.php?lang={code}: locale persists on the login page")]
    [TestCaseSource(nameof(LoginPageCases))]
    public async Task LoginPage_Lang_PersistsLocale(LocaleExpectation expectation)
    {
        var (status, body) = await MoodledemoHttp.GetAsync(MoodledemoHttp.LoginWithLang(expectation.Code));

        status.Should().Be(HttpStatusCode.OK);
        var (lang, dir) = HtmlLocaleReader.ReadLocale(body);
        lang.Should().Be(expectation.Lang);
        dir.Should().Be(expectation.Dir);
        HtmlLocaleReader.ReadTitle(body).Should().Be(expectation.Title);
        body.Should().Contain("name=\"username\"", "the login form must be present in the page markup");
        body.Should().Contain("id=\"loginbtn\"", "the login submit button must be present in the page markup");
    }

    [Test]
    [Category("Smoke")]
    [Description("3.1 Unknown ?lang value: 200 with an English fallback (no server error)")]
    public async Task Home_UnknownLang_FallsBackToEnglish()
    {
        var (status, body) = await MoodledemoHttp.GetAsync(
            MoodledemoHttp.HomeWithLang(MoodledemoEndpoints.UnknownLangCode));

        status.Should().Be(HttpStatusCode.OK, "an unknown language code must never break the page");
        var (lang, dir) = HtmlLocaleReader.ReadLocale(body);
        var english = MoodledemoFixtures.English;
        lang.Should().Be(english.Lang);
        dir.Should().Be(english.Dir);
    }
}
