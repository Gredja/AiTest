using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;
using static Core.Helpers.ParamHelper;

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
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

        if (response.Data!.Count == 0)
        {
            var create = await Post<CreateIssueModelRequest, IssueModelResponse>(
                GitHubEndpoints.RepoIssues,
                new CreateIssueModelRequest { Title = TestTitle },
                TestRepoParam());
            _createdIssueNumber = create.Data!.Number;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdIssueNumber.HasValue)
        {
            try
            {
                var delete = await Delete<object>($"{GitHubEndpoints.RepoIssues}/{_createdIssueNumber}",
                    TestRepoParam());

                if (!delete.IsSuccessful)
                {
                    TestContext.Progress.WriteLine(
                        $"Warning: issue {_createdIssueNumber} not deleted: HTTP {(int)delete.StatusCode}");
                }
            }
            catch (HttpRequestException exception)
            {
                TestContext.Progress.WriteLine(
                    $"Warning: failed to delete issue {_createdIssueNumber}: {exception.Message}");
            }
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
        response.Data.Should().NotBeEmpty("Setup should have guaranteed at least one issue");
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("3.3 Each issue has valid fields")]
    public async Task GetIssues_HasValidFields()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

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
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

        var ids = response.Data!.Select(issue => issue.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("3.5 Each issue has non-empty state")]
    public async Task GetIssues_EachIssueHasValidState()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            TestRepoParam());

        response.Data.Should().OnlyContain(issue => issue.State == GitHubEndpoints.StateOpen || issue.State == GitHubEndpoints.StateClosed);
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

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.11 Filter by state=open returns only open issues")]
    public async Task GetIssues_FilterByStateOpen()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateOpen)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(issue => issue.State == GitHubEndpoints.StateOpen);
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

    private const string InvalidQueryValue = "abc";

    [Test]
    [Category("Negative")]
    [Description("3.13 since=abc returns 422")]
    public async Task GetIssues_InvalidSince_Returns422()
    {
        var response = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), Query("since", InvalidQueryValue)]);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }
}
