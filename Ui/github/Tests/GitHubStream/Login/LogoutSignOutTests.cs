using Core.Config;
using FluentAssertions;
using Ui.Github.Tests.Support;

namespace Ui.Github.Tests.GitHubStream.Login;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class LogoutSignOutTests : GitHubUiTestBase
{
    // Signing out must never touch the shared storageState file: a fresh anonymous context,
    // and no SaveStorageStateAsync — a logout in the shared session would break every other UI test
    protected override bool IsStorageStateEnabled => false;

    [Test]
    [Category("Smoke")]
    [Description("UI-2 Sign out via user menu: session ends, header offers Sign in")]
    [Ignore("GitHub device-verification wall: the prerequisite fresh login lands on sessions/verified-device (email code) — needs one human verification + .auth/github-device-trust.json; observed 2026-11-09, UI-OB UI-1")]
    public async Task Logout_SignOut_EndsSession()
    {
        await Browser.LoginPage.OpenAsync();
        await Browser.LoginPage.SignInAsync(TestConfig.UiLogin, TestConfig.UiPassword);

        await Browser.HeaderPage.SignOutAsync();

        (await Browser.HeaderPage.IsSignedOutAsync()).Should().BeTrue("sign out must end the session");
    }
}
