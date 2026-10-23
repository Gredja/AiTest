using FluentAssertions;
using Ui.Github.Tests.Support;

namespace Ui.Github.Tests.GitHubStream.Login;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class AnonAccessUiTests : GitHubUiTestBase
{
    // both tests exercise the login wall — a fresh anonymous context, never the shared
    // storageState (same rule as LogoutSignOutTests: touching the shared session breaks the run)
    protected override bool IsStorageStateEnabled => false;

    [Test]
    [Category("Negative")]
    [Description("UI-22 Anonymous issue create: redirected to the login page")]
    public async Task AnonNewIssue_ViaUI_RedirectsToLogin()
    {
        await Browser.IssueCreatePage.OpenAsync();

        var login = await Browser.LoginPage.WaitForLoginRedirectAsync();
        login.IsSuccess.Should().BeTrue(
            $"anonymous create must land on /login (attempts={login.Attempts}, elapsed={login.Elapsed})");
    }

    [Test]
    [Category("Negative")]
    [Description("UI-22 Anonymous new pull request: compare page gates behind Sign in")]
    public async Task AnonNewPull_ViaUI_ShowsSignInGate()
    {
        await Browser.PullsPage.OpenAsync();
        await Browser.PullsPage.ClickNewPullRequestAsync();

        (await Browser.LoginPage.HasSignInLinkAsync()).Should().BeTrue(
            "the compare page must offer Sign in instead of the create form");
    }
}
