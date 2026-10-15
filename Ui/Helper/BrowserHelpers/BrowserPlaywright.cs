using Microsoft.Playwright;
using Ui.Helper.UiHelper.CommonPages;
using Ui.Helper.UiHelper.GitHubStream.Issues;
using Ui.Helper.UiHelper.GitHubStream.Repository;

namespace Ui.Helper.BrowserHelpers;

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
