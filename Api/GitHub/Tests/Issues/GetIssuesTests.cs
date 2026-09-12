using NUnit.Framework;
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
    [Test]
    [Category("HealthCheck")]
    [Description("3.1 GET /repos/{owner}/{repo}/issues returns 200 OK")]
    public async Task GetIssues_ReturnsOk()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModel>>(GitHubEndpoints.RepoIssues, Method.Get,
            RepoParam(owner, repo));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("3.2 Each issue has valid fields")]
    public async Task GetIssues_HasValidFields()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModel>>(GitHubEndpoints.RepoIssues, Method.Get,
            RepoParam(owner, repo));

        response.Data.Should().NotBeNull();
        if (response.Data!.Count > 0)
        {
            var issue = response.Data.First();
            issue.Id.Should().BeGreaterThan(0);
            issue.Title.Should().NotBeNullOrWhiteSpace();
            issue.State.Should().NotBeNullOrWhiteSpace();
            issue.User.Should().NotBeNull();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("3.3 Content-Type is application/json")]
    public async Task GetIssues_ContentTypeIsJson()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModel>>(GitHubEndpoints.RepoIssues, Method.Get,
            RepoParam(owner, repo));

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.4 Pagination works with per_page and page params")]
    public async Task GetIssues_PaginationWorks()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModel>>(GitHubEndpoints.RepoIssues, Method.Get,
            [.. RepoParam(owner, repo), .. PaginationParams(1, 5)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Test]
    [Category("Performance")]
    [Description("3.5 Response time < 5 seconds")]
    public async Task GetIssues_ResponseTimeIsAcceptable()
    {
        var (owner, repo) = ParseRepo();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<List<IssueModel>>(GitHubEndpoints.RepoIssues, Method.Get,
            RepoParam(owner, repo));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(GitHubEndpoints.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("3.6 Filter by state=open returns only open issues")]
    public async Task GetIssues_FilterByStateOpen()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModel>>(GitHubEndpoints.RepoIssues, Method.Get,
            [.. RepoParam(owner, repo), .. StateParam("open")]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().OnlyContain(i => i.State == "open");
    }

    [Test]
    [Category("Smoke")]
    [Description("3.7 Filter by state=closed returns only closed issues")]
    public async Task GetIssues_FilterByStateClosed()
    {
        var (owner, repo) = ParseRepo();
        var response = await Get<List<IssueModel>>(GitHubEndpoints.RepoIssues, Method.Get,
            [.. RepoParam(owner, repo), .. StateParam("closed")]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        if (response.Data!.Count > 0)
        {
            response.Data.Should().OnlyContain(i => i.State == "closed");
        }
    }
}
