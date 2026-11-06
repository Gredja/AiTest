using Core.Models.GitHub;
using Core.Generators.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class CreateIssueTests : GitHubTestBase
{
    private readonly List<int> _createdIssueNumbers = [];
    private const int LongTitleLength = 65536;
    private static readonly List<string> TestIssueLabels = ["bug", "urgent"];
    private static readonly string MarkdownBody = "# Heading\n\n**bold** and <b>html</b> [link](https://example.com)";

    [Test]
    [Category("HealthCheck")]
    [Description("17.7 POST create issue returns 201 Created with id and number > 0")]
    public async Task CreateIssue_ReturnsCreated()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, IssueGenerator.CreateRandom("Create issue", "Body"), TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);
        created.Data.Should().NotBeNull();
        created.Data!.Id.Should().BeGreaterThan(0, "created issue must get a server id");
        created.Data!.Number.Should().BeGreaterThan(0, "created issue must get a server number");
    }

    [Test]
    [Category("ContractCheck")]
    [Description("17.8 Response matches expected model schema (ShouldHaveValidContract)")]
    public async Task CreateIssue_ResponseMatchesContract()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, IssueGenerator.CreateRandom("Contract probe", "Body"), TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);
        created.Data.Should().NotBeNull();
        created.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Regression")]
    [Description("17.9 Response title, body and labels match the request (ShouldMatchRequest)")]
    public async Task CreateIssue_MatchesRequest()
    {
        var request = IssueGenerator.CreateRandom("Match probe", "Match body");
        request.Labels = TestIssueLabels;

        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, request, TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);

        var matchRequest = IssueGenerator.CreateWithTitle(request.Title);
        matchRequest.Body = request.Body;
        created.Data!.ShouldMatchRequest(matchRequest);
        created.Data!.Labels.Should().NotBeNull("response must echo the labels sent in the request");
        created.Data!.Labels.Select(label => label.Name).Should().BeEquivalentTo(TestIssueLabels,
            "response label names must match the request labels — request carries strings, response carries Label objects");
    }

    [Test]
    [Category("Smoke")]
    [Description("17.10 Content-Type is application/json")]
    public async Task CreateIssue_ContentTypeIsJson()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, IssueGenerator.CreateRandom("Content type probe", "Body"), TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);
        created.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("17.11 Minimal payload with only required title returns 201")]
    public async Task CreateIssue_TitleOnly_ReturnsCreated()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, IssueGenerator.CreateRandom("Title only probe"), TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);
        created.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("17.12 Labels array is accepted and echoed in the response")]
    public async Task CreateIssue_WithLabels_ReturnsCreated()
    {
        var request = IssueGenerator.CreateRandom("Labels probe", "Labels body");
        request.Labels = TestIssueLabels;

        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, request, TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);
        created.Data.Should().NotBeNull();
        created.Data!.Labels.Should().NotBeNull("response must echo the labels sent in the request");
        created.Data!.Labels.Select(label => label.Name).Should().BeEquivalentTo(TestIssueLabels,
            "response label names must match the request labels");
    }

    [Test]
    [Category("Regression")]
    [Description("17.13 Created issue appears in GET issues list (read-after-write)")]
    public async Task CreateIssue_VisibleInList()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, IssueGenerator.CreateRandom("Visibility list probe", "Body"), TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);
        created.Data.Should().NotBeNull();

        var result = await WaitHelper.WaitUntilAsync(
            () => Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
                [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data is not null
                && response.Data.Any(issue => issue.Number == created.Data!.Number));

        result.IsSuccess.Should().BeTrue(
            $"created issue must be visible in the issues list; elapsed {result.Elapsed}, attempts {result.Attempts}");
    }

    [Test]
    [Category("Performance")]
    [Description("17.15 Create response time within the write budget")]
    public async Task CreateIssue_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, IssueGenerator.CreateRandom("Timing probe", "Body"), TestRepoParam());
        stopwatch.Stop();
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);
        AssertHelper.ShouldBeWithinResponseBudget(stopwatch.ElapsedMilliseconds);
    }

    [Test]
    [Category("Smoke")]
    [Description("17.16 Edge: 65536-char title returns 201 or 422")]
    public async Task CreateIssue_LongTitle_ReturnsCreatedOrUnprocessable()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, IssueGenerator.CreateWithTitle(new string('a', LongTitleLength)), TestRepoParam());
        created.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.UnprocessableEntity);
        if (created.StatusCode == HttpStatusCode.Created)
        {
            _createdIssueNumbers.Add(created.Data!.Number);
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("17.17 Edge: markdown/HTML in body returns 201")]
    public async Task CreateIssue_SpecialCharsBody_ReturnsCreated()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, IssueGenerator.CreateRandom("Special chars probe", MarkdownBody), TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);
        created.Data.Should().NotBeNull();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var issueNumber in _createdIssueNumbers)
        {
            await CleanupIssueAsync(issueNumber);
        }
    }
}
