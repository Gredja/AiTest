using Microsoft.Playwright;
using Ui.Github.Pages.Common;
using Ui.Github.Pages.GitHubStream.Issues;
using Ui.Github.Pages.GitHubStream.Pulls;
using Ui.Github.Pages.GitHubStream.Repository;

namespace Ui.Github.Tests.Support;

internal sealed class BrowserPlaywright(IPage page)
{
    internal LoginPage LoginPage { get; } = new(page);
    internal HeaderPage HeaderPage { get; } = new(page);
    internal ProfilePage ProfilePage { get; } = new(page);
    internal AccountSettingsPage AccountSettingsPage { get; } = new(page);
    internal RepositoryPage RepositoryPage { get; } = new(page);
    internal IssuesPage IssuesPage { get; } = new(page);
    internal PullsPage PullsPage { get; } = new(page);
    internal PullDetailPage PullDetailPage { get; } = new(page);
    internal PullCreatePage PullCreatePage { get; } = new(page);
    internal BranchesPage BranchesPage { get; } = new(page);
    internal CodePage CodePage { get; } = new(page);
    internal NotFoundPage NotFoundPage { get; } = new(page);
    internal IssueCreatePage IssueCreatePage { get; } = new(page);
    internal IssueDetailPage IssueDetailPage { get; } = new(page);
    internal CommentForm CommentForm { get; } = new(page);
}
