using Microsoft.Playwright;
using Ui.Github.Pages.Common;
using Ui.Github.Pages.GitHubStream.Issues;
using Ui.Github.Pages.GitHubStream.Pulls;
using Ui.Github.Pages.GitHubStream.Repository;

namespace Ui.Github.Tests.Support;

internal sealed class BrowserPlaywright
{
    internal BrowserPlaywright(IPage page)
    {
        Page = page;
        LoginPage = new LoginPage(page);
        HeaderPage = new HeaderPage(page);
        ProfilePage = new ProfilePage(page);
        AccountSettingsPage = new AccountSettingsPage(page);
        RepositoryPage = new RepositoryPage(page);
        IssuesPage = new IssuesPage(page);
        PullsPage = new PullsPage(page);
        PullDetailPage = new PullDetailPage(page);
        PullCreatePage = new PullCreatePage(page);
        BranchesPage = new BranchesPage(page);
        CodePage = new CodePage(page);
        NotFoundPage = new NotFoundPage(page);
        IssueCreatePage = new IssueCreatePage(page);
        IssueDetailPage = new IssueDetailPage(page);
        CommentForm = new CommentForm(page);
    }

    internal IPage Page { get; }
    internal LoginPage LoginPage { get; }
    internal HeaderPage HeaderPage { get; }
    internal ProfilePage ProfilePage { get; }
    internal AccountSettingsPage AccountSettingsPage { get; }
    internal RepositoryPage RepositoryPage { get; }
    internal IssuesPage IssuesPage { get; }
    internal PullsPage PullsPage { get; }
    internal PullDetailPage PullDetailPage { get; }
    internal PullCreatePage PullCreatePage { get; }
    internal BranchesPage BranchesPage { get; }
    internal CodePage CodePage { get; }
    internal NotFoundPage NotFoundPage { get; }
    internal IssueCreatePage IssueCreatePage { get; }
    internal IssueDetailPage IssueDetailPage { get; }
    internal CommentForm CommentForm { get; }
}
