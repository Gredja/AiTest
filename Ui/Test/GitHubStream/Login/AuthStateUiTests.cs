using FluentAssertions;
using AllureAdapter;
using Ui.Helper;

namespace Ui.Test.GitHubStream.Login;

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

        menuHeading.Should().Contain("gredjapl", "UI session must be signed in as gredjapl");
    }
}
