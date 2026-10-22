using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class LoginLocalizationUiTests : MoodledemoTestBase
{
    private static IEnumerable<TestCaseData> LabelCases()
    {
        foreach (var labels in MoodledemoFixtures.LoginLabels)
        {
            yield return new TestCaseData(labels)
                .SetName($"LoginPage_Labels_MatchFixture({labels.Code})");
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("8.1 Login labels and submit button match the localized fixture")]
    [TestCaseSource(nameof(LabelCases))]
    public async Task LoginPage_Labels_MatchFixture(LoginLabelExpectation labels)
    {
        await Browser.LoginPage.GotoAsync(labels.Code);

        var usernameLabel = await Browser.LoginPage.ReadUsernameLabelAsync();
        usernameLabel.Should().Be(labels.Username, "username label must be localized for {0}", labels.Code);

        var passwordLabel = await Browser.LoginPage.ReadPasswordLabelAsync();
        passwordLabel.Should().Be(labels.Password, "password label must be localized for {0}", labels.Code);

        var loginButtonText = await Browser.LoginPage.ReadLoginButtonTextAsync();
        loginButtonText.Should().Be(labels.LoginButton, "submit button must be localized for {0}", labels.Code);
    }
}
