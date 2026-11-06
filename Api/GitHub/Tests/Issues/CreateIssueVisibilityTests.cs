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
public class CreateIssueVisibilityTests : GitHubTestBase
{
    private readonly List<int> _createdIssueNumbers = [];

    [Test]
    [Category("Performance")]
    [Description("17.1 Created issue becomes visible within max response time")]
    public async Task CreateIssue_RecordVisibleWithinTimeLimit()
    {
        var request = IssueGenerator.CreateRandom("Visibility probe", "Body");

        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, request, TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIssueNumbers.Add(created.Data!.Number);
        created.Data.Should().NotBeNull();

        var result = await WaitHelper.WaitUntilAsync(
            () => Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
                [.. TestRepoParam(), .. IssueNumberParam(created.Data!.Number)]),
            response => response.StatusCode == HttpStatusCode.OK && response.Data is not null,
            timeout: TimeSpan.FromMilliseconds(TestConfig.MaxResponseTimeMs));

        result.IsSuccess.Should().BeTrue(
            $"created issue not visible within {TestConfig.MaxResponseTimeMs} ms — {WaitDiagnosticsHelper.Describe(result)}");
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
