using Microsoft.Playwright;
using Ui.Pages.Common;
using Ui.Pages.GitHubStream.Issues;
using Ui.Pages.GitHubStream.Repository;

namespace Ui.Tests.Support;

internal sealed class BrowserPlaywright
{
    internal BrowserPlaywright(IPage page)
    {
        Page = page;
        LoginPage = new LoginPage(page);
        HeaderPage = new HeaderPage(page);
        RepositoryPage = new RepositoryPage(page);
        IssuesPage = new IssuesPage(page);
        PullsPage = new PullsPage(page);
        CodePage = new CodePage(page);
        NotFoundPage = new NotFoundPage(page);
        IssueCreatePage = new IssueCreatePage(page);
        IssueDetailPage = new IssueDetailPage(page);
        CommentForm = new CommentForm(page);
    }

    internal IPage Page { get; }
    internal LoginPage LoginPage { get; }
    internal HeaderPage HeaderPage { get; }
    internal RepositoryPage RepositoryPage { get; }
    internal IssuesPage IssuesPage { get; }
    internal PullsPage PullsPage { get; }
    internal CodePage CodePage { get; }
    internal NotFoundPage NotFoundPage { get; }
    internal IssueCreatePage IssueCreatePage { get; }
    internal IssueDetailPage IssueDetailPage { get; }
    internal CommentForm CommentForm { get; }
}
