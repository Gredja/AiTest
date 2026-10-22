using Core.Config;
using FluentAssertions;
using Ui.Moodledemo.Tests.Support;

namespace Ui.Moodledemo.Tests;

[TestFixture]
[Category("MoodleDemo")]
public class AccountUiTests : MoodledemoTestBase
{
    private static IEnumerable<TestCaseData> AccountCases()
    {
        yield return new TestCaseData(TestConfig.MoodleDemoStudentUsername)
            .SetName("Student_LoginShowsDashboard_LogoutReturnsGuest");
        yield return new TestCaseData(TestConfig.MoodleDemoTeacherUsername)
            .SetName("Teacher_LoginShowsDashboard_LogoutReturnsGuest");
        yield return new TestCaseData(TestConfig.MoodleDemoManagerUsername)
            .SetName("Manager_LoginShowsDashboard_LogoutReturnsGuest");
        yield return new TestCaseData(TestConfig.MoodleDemoParentUsername)
            .SetName("Parent_LoginShowsDashboard_LogoutReturnsGuest");
        yield return new TestCaseData(TestConfig.MoodleDemoPrivacyOfficerUsername)
            .SetName("PrivacyOfficer_LoginShowsDashboard_LogoutReturnsGuest");
    }

    [Test]
    [Category("Smoke")]
    [Description("L3.1 Demo account logs in, sees the dashboard, logs out back to guest")]
    [TestCaseSource(nameof(AccountCases))]
    public async Task DemoAccount_LoginShowsDashboard_LogoutReturnsGuest(string username)
    {
        await Browser.LoginPage.GotoAsync();
        await Browser.LoginPage.LoginAsAsync(username, TestConfig.MoodleDemoPassword);

        (await Browser.LoginPage.IsLoggedInAsync()).Should().BeTrue(
            "after login the logout link must be present in the DOM for {0}", username);

        var url = Browser.Page.Url;
        url.Should().Contain(
            MoodledemoEndpoints.MyDashboardPath.TrimEnd('/'),
            "login must land on the dashboard for {0}", username);

        await Browser.LoginPage.LogoutAsync();

        (await Browser.LoginPage.IsLoggedInAsync()).Should().BeFalse(
            "after logout the guest state must return for {0}", username);
    }
}
