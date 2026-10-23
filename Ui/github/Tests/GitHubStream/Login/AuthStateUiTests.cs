using Core.Config;
using FluentAssertions;
using Ui.Github.Tests.Support;

namespace Ui.Github.Tests.GitHubStream.Login;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class AuthStateUiTests : GitHubUiTestBase
{
    [Test]
    [Category("Smoke")]
    [Description("UI-1 Auth state: user menu shows gredjapl")]
    public async Task AuthState_UserMenu_ShowsGredjapl()
    {
        await Browser.RepositoryPage.OpenAsync();

        var menuHeading = await Browser.HeaderPage.GetSignedInUsernameAsync();

        menuHeading.Should().Contain(TestConfig.UiUsername, "UI session must be signed in as the configured UI account");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-15 User menu → Profile: profile page shows username heading")]
    public async Task Profile_ViaUI_ShowsGredjapl()
    {
        await Browser.RepositoryPage.OpenAsync();
        await Browser.HeaderPage.OpenProfileAsync();
        var found = await Browser.ProfilePage.WaitForUsernameHeadingAsync();

        found.IsSuccess.Should().BeTrue(
            $"profile must show the username heading (attempts={found.Attempts}, elapsed={found.Elapsed})");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-16 User menu → Settings: account settings shows account heading")]
    public async Task Settings_ViaUI_ShowsAccount()
    {
        await Browser.RepositoryPage.OpenAsync();
        await Browser.HeaderPage.OpenSettingsAsync();
        var found = await Browser.AccountSettingsPage.WaitForAccountHeadingAsync();

        found.IsSuccess.Should().BeTrue(
            $"settings must show the account heading (attempts={found.Attempts}, elapsed={found.Elapsed})");
    }
}
