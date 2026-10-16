using FluentAssertions;
using AllureAdapter;
using Ui.Helper;

namespace Ui.Tests.GitHubStream.Repository;

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class RepositoryUiTests : GitHubUiTestBase
{
    [Test]
    [Category("Smoke")]
    [Description("UI-3 Repository landing: default branch main, README visible")]
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
    [Description("UI-8 Code view: file tree contains README.md")]
    public async Task CodeView_ReadmeVisible_Verify()
    {
        await Browser.CodePage.OpenAsync();

        var hasReadme = await Browser.CodePage.HasReadmeAsync();
        hasReadme.Should().BeTrue("tree/main must list README.md");
    }

    [Test]
    [Category("Negative")]
    [Description("UI-9 Non-existent repo: 404 illustration message shown")]
    public async Task NotFoundPage_Shows404Message()
    {
        await Browser.NotFoundPage.OpenAsync();

        var notFound = await Browser.NotFoundPage.WaitFor404MessageAsync();
        notFound.IsSuccess.Should().BeTrue(
            $"GitHub serves the 404 illustration for missing repos (attempts={notFound.Attempts}, elapsed={notFound.Elapsed})");
    }
}
