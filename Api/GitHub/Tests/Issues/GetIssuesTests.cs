using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;
using static Core.Helpers.ParamHelper;
using static Core.Helpers.TestDataConstants;

namespace Api.GitHub.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetIssuesTests : GitHubTestBase
{
    private static readonly TimeSpan _stateOpenConsistencyTimeout = TimeSpan.FromSeconds(10);
    private int? _createdIssueNumber;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());
        response.ShouldHaveStatusCode(HttpStatusCode.OK);

        if (response.Data!.Count == 0)
        {
            var create = await Post<CreateIssueModelRequest, IssueModelResponse>(
                GitHubEndpoints.RepoIssues,
                new CreateIssueModelRequest { Title = IssueContractCheckTitle },
                TestRepoParam());
            create.ShouldHaveStatusCode(HttpStatusCode.Created);
            _createdIssueNumber = create.Data!.Number;
        }
    }

    [Test]
    [Category("HealthCheck")]
    [Description("3.1 GET /repos/{owner}/{repo}/issues returns 200 OK")]
    public async Task GetIssues_ReturnsOk()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("3.2 Response matches expected contract")]
    public async Task GetIssues_ResponseMatchesContract()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty(IssueListNotEmptyMessage);
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("3.3 Each issue has valid fields")]
    public async Task GetIssues_HasValidFields()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.ForEach(issue => issue.ShouldHaveValidFields());
    }

    [Test]
    [Category("Regression")]
    [Description("3.4 All issue IDs are unique")]
    public async Task GetIssues_AllIdsAreUnique()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        var ids = response.Data!.Select(issue => issue.Id);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("3.5 Each issue has non-empty state")]
    public async Task GetIssues_EachIssueHasValidState()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(issue => issue.State == GitHubEndpoints.StateOpen || issue.State == GitHubEndpoints.StateClosed);
    }

    [Test]
    [Category("Negative")]
    [Description("3.6 Non-existent repo returns 404")]
    public async Task GetIssues_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("3.7 Invalid state param returns 422")]
    public async Task GetIssues_InvalidState_Returns422()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam("invalid")]);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.8 Content-Type is application/json")]
    public async Task GetIssues_ContentTypeIsJson()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.9 Pagination works with per_page and page params")]
    public async Task GetIssues_PaginationWorks()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. PaginationParams(FirstPage, DefaultPageSize)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(DefaultPageSize);
    }

    [Test]
    [Category("Performance")]
    [Description("3.10 Response time < 5 seconds")]
    public async Task GetIssues_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());
        stopwatch.Stop();

        AssertHelper.ShouldBeWithinResponseBudget(stopwatch.ElapsedMilliseconds);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.11 Filter by state=open returns only open issues")]
    public async Task GetIssues_FilterByStateOpen()
    {
        // GitHub list may serve a torn read while E2E closes issues concurrently (solution run):
        // a just-closed issue briefly appears in ?state=open — refresh until the read is consistent
        var result = await WaitHelper.WaitUntilAsync(
            () => Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
                [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateOpen)]),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data is not null
                && response.Data.All(issue => issue.State == GitHubEndpoints.StateOpen),
            timeout: _stateOpenConsistencyTimeout);

        result.IsSuccess.Should().BeTrue(
            $"state=open must return only open issues; waited {result.Elapsed}, attempts {result.Attempts}, last status {result.LastValue?.StatusCode}");
        result.LastValue!.Data.Should().OnlyContain(issue => issue.State == GitHubEndpoints.StateOpen);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.12 Filter by state=closed returns only closed issues")]
    public async Task GetIssues_FilterByStateClosed()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateClosed)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(issue => issue.State == GitHubEndpoints.StateClosed);
    }

    [Test]
    [Category("Negative")]
    [Description("3.13 since=abc returns 422")]
    public async Task GetIssues_InvalidSince_Returns422()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), Query(JsonFields.Since, NonNumericValue)]);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Regression")]
    [Description("3.14 Issues list includes pull requests (pull_request field present)")]
    public async Task GetIssues_ItemsIncludePullRequests()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Any(issue => issue.PullRequest is not null).Should().BeTrue(
            "GitHub serves pull requests through the issues endpoint — OB §6 gotcha");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() =>
        await CleanupIssueAsync(_createdIssueNumber);
}
