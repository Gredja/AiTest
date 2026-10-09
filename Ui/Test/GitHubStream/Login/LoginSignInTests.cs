using AllureAdapter;
using Core.Config;
using FluentAssertions;
using Ui.Helper;

namespace Ui.Test.GitHubStream.Login;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class LoginSignInTests : GitHubUiTestBase
{
    // storageState would already be signed in and /login would redirect to the
    // dashboard before the credentials are ever entered
    protected override bool IsStorageStateEnabled => false;

    [Test]
    [Category("Smoke")]
    [Description("UI-1 Ordinary login with credentials signs in and saves storageState")]
    public async Task Login_SignIn_SavesStorageState()
    {
        await Browser.LoginPage.OpenAsync();
        await Browser.LoginPage.SignInAsync(TestConfig.UiLogin, TestConfig.UiPassword);

        var menuHeading = await Browser.HeaderPage.GetSignedInUsernameAsync();
        menuHeading.Should().Contain(TestConfig.UiUsername, "login must land on the gredjapl account");

        await SaveStorageStateAsync();
    }
}
