using FluentAssertions;
using AllureAdapter;
using Ui.Helper;

namespace Ui.Test.GitHubStream.Repository;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class RepositoryUiTests : GitHubUiTestBase
{
    [Test]
    [Category("Smoke")]
    [Description("UI-2 Repository landing: default branch main, README visible")]
    public async Task RepositoryLanding_DefaultBranchAndReadme_Verify()
    {
        await Browser.RepositoryPage.OpenAsync();

        var branch = await Browser.RepositoryPage.GetDefaultBranchAsync();
        branch.Should().Contain("main", "sandbox repo default branch is main");

        var hasReadme = await Browser.RepositoryPage.HasReadmeAsync();
        hasReadme.Should().BeTrue("repo root must list README.md");
    }

    [Test]
    [Category("Smoke")]
    [Description("UI-5 Code view: file tree contains README.md")]
    public async Task CodeView_ReadmeVisible_Verify()
    {
        await Browser.CodePage.OpenAsync();

        var hasReadme = await Browser.CodePage.HasReadmeAsync();
        hasReadme.Should().BeTrue("tree/main must list README.md");
    }

    [Test]
    [Category("Negative")]
    [Description("UI-7 Non-existent repo: 404 illustration message shown")]
    public async Task NotFoundPage_Shows404Message()
    {
        await Browser.NotFoundPage.OpenAsync();

        var has404 = await Browser.NotFoundPage.Has404MessageAsync();
        has404.Should().BeTrue("GitHub serves the 404 illustration for missing repos");
    }
}
