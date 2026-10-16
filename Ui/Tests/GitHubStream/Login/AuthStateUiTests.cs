using Core.Config;
using FluentAssertions;
using AllureAdapter;
using Ui.Helper;

namespace Ui.Tests.GitHubStream.Login;

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
}
