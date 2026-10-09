using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;
using static Core.Helpers.ParamHelper;

namespace Api.GitHub.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetIssueByIdTests : GitHubTestBase
{
    private const int ExistingIssueNumber = 5;
    private const string NonNumericIssueNumber = "abc";
    private const string TestTitle = "Test issue for contract check";

    private int? _createdIssueNumber;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var issues = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        issues.ShouldHaveStatusCode(HttpStatusCode.OK);

        if (issues.Data!.Count == 0)
        {
            var create = await Post<CreateIssueModelRequest, IssueModelResponse>(
                GitHubEndpoints.RepoIssues,
                new CreateIssueModelRequest { Title = TestTitle },
                TestRepoParam());
            create.ShouldHaveStatusCode(HttpStatusCode.Created);
            _createdIssueNumber = create.Data!.Number;
        }
    }

    [Test]
    [Category("HealthCheck")]
    [Description("10.1 GET /repos/{owner}/{repo}/issues/{number} returns 200 OK")]
    public async Task GetIssueById_ReturnsOk()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("10.2 Response matches expected contract")]
    public async Task GetIssueById_ResponseMatchesContract()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("10.3 Response has valid issue fields")]
    public async Task GetIssueById_HasValidFields()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Negative")]
    [Description("10.4 Non-existent issue returns 404")]
    public async Task GetIssueById_NonExistentIssue_ReturnsNotFound()
    {
        var issues = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        issues.ShouldHaveStatusCode(HttpStatusCode.OK);
        issues.Data.Should().NotBeEmpty("Setup should have guaranteed at least one issue");
        var maxIssueNumber = issues.Data!.Max(issue => issue.Number);
        var nonExistentIssueNumber = maxIssueNumber + GitHubEndpoints.NonExistentIdOffset;

        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(nonExistentIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("10.5 Issue number 0 returns 404")]
    public async Task GetIssueById_ZeroIssueNumber_ReturnsNotFound()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(0)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("10.6 Negative issue number returns 404")]
    public async Task GetIssueById_NegativeIssueNumber_ReturnsNotFound()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(-1)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("10.7 Title matches expected issue")]
    public async Task GetIssueById_TitleIsNotEmpty()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        response.Data!.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Regression")]
    [Description("10.8 Repeated calls return same data")]
    public async Task GetIssueById_RepeatedCalls_ReturnSameData()
    {
        var response1 = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);
        var response2 = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Title.Should().Be(response2.Data!.Title);
    }

    [Test]
    [Category("Negative")]
    [Description("10.9 Non-numeric issue number returns 404")]
    public async Task GetIssueById_InvalidSegment_ReturnsNotFound()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), UrlSegment("issue_number", NonNumericIssueNumber)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("10.10 Issue number int.MaxValue returns 404")]
    public async Task GetIssueById_IntMaxIssueNumber_ReturnsNotFound()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(int.MaxValue)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Smoke")]
    [Description("10.11 Content-Type is application/json")]
    public async Task GetIssueById_ContentTypeIsJson()
    {
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Performance")]
    [Description("10.12 Response time < 5 seconds")]
    public async Task GetIssueById_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            [.. TestRepoParam(), .. IssueNumberParam(ExistingIssueNumber)]);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdIssueNumber.HasValue)
        {
            try
            {
                // REST has no issue-delete endpoint (DELETE → 404, see OB §17) — cleanup closes instead
                var cleanup = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
                    GitHubEndpoints.RepoIssueById,
                    new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed },
                    [.. TestRepoParam(), .. IssueNumberParam(_createdIssueNumber.Value)]);

                if (!cleanup.IsSuccessful)
                {
                    TestContext.Progress.WriteLine(
                        $"Warning: issue {_createdIssueNumber} not closed: HTTP {(int)cleanup.StatusCode}");
                }
            }
            catch (HttpRequestException exception)
            {
                TestContext.Progress.WriteLine(
                    $"Warning: failed to close issue {_createdIssueNumber}: {exception.Message}");
            }
        }
    }
}
