using RestSharp;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetIssuesTests : GitHubTestBase
{
    private const string TestTitle = "Test issue for contract check";
    private int? _createdIssueNumber;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(owner, repo));

        if (response.Data!.Count == 0)
        {
            var create = await Post<CreateIssueModelRequest, IssueModelResponse>(
                GitHubEndpoints.RepoIssues,
                new CreateIssueModelRequest { Title = TestTitle },
                RepoParam(owner, repo));
            _createdIssueNumber = create.Data!.Number;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdIssueNumber.HasValue)
        {
            var (owner, repo) = ParseRepo();
            await Delete<object>($"{GitHubEndpoints.RepoIssues}/{_createdIssueNumber}",
                RepoParam(owner, repo));
        }
    }

    [Test]
    [Category("HealthCheck")]
    [Description("3.1 GET /repos/{owner}/{repo}/issues returns 200 OK")]
    public async Task GetIssues_ReturnsOk()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(owner, repo));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("3.2 Response matches expected contract")]
    public async Task GetIssues_ResponseMatchesContract()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(owner, repo));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty("Setup should have guaranteed at least one issue");
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("3.3 Each issue has valid fields")]
    public async Task GetIssues_HasValidFields()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(owner, repo));

        foreach (var issue in response.Data!)
        {
            issue.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("3.4 All issue IDs are unique")]
    public async Task GetIssues_AllIdsAreUnique()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(owner, repo));

        var ids = response.Data!.Select(i => i.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("3.5 Each issue has non-empty state")]
    public async Task GetIssues_EachIssueHasValidState()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(owner, repo));

        response.Data.Should().OnlyContain(i => i.State == GitHubEndpoints.StateOpen || i.State == GitHubEndpoints.StateClosed);
    }

    [Test]
    [Category("Negative")]
    [Description("3.6 Non-existent repo returns 404")]
    public async Task GetIssues_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("3.7 Invalid state param returns 422")]
    public async Task GetIssues_InvalidState_Returns422()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. RepoParam(owner, repo), .. StateParam("invalid")]);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.8 Content-Type is application/json")]
    public async Task GetIssues_ContentTypeIsJson()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(owner, repo));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.9 Pagination works with per_page and page params")]
    public async Task GetIssues_PaginationWorks()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. RepoParam(owner, repo), .. PaginationParams(1, 5)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Test]
    [Category("Performance")]
    [Description("3.10 Response time < 5 seconds")]
    public async Task GetIssues_ResponseTimeIsAcceptable()
    {
        var (owner, repo) = ParseRepo();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(owner, repo));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.11 Filter by state=open returns only open issues")]
    public async Task GetIssues_FilterByStateOpen()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. RepoParam(owner, repo), .. StateParam(GitHubEndpoints.StateOpen)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(i => i.State == GitHubEndpoints.StateOpen);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.12 Filter by state=closed returns only closed issues")]
    public async Task GetIssues_FilterByStateClosed()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. RepoParam(owner, repo), .. StateParam(GitHubEndpoints.StateClosed)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(i => i.State == GitHubEndpoints.StateClosed);
    }
}
