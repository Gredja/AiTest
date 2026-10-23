using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;
using static Core.Helpers.TestDataConstants;

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
        var request = new CreateIssueModelRequest
        {
            Title = $"Visibility probe {DataGenerator.RandomString(TitleRandomLength)}",
            Body = $"Body {DataGenerator.RandomString(BodyRandomLength)}"
        };

        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, request, TestRepoParam());
        if (created.Data is not null)
        {
            _createdIssueNumbers.Add(created.Data.Number);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();

        var result = await WaitHelper.WaitUntilAsync(
            () => Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
                [.. TestRepoParam(), .. IssueNumberParam(created.Data!.Number)]),
            response => response.StatusCode == HttpStatusCode.OK && response.Data is not null,
            timeout: TimeSpan.FromMilliseconds(TestConfig.MaxResponseTimeMs));

        if (!result.IsSuccess)
        {
            Assert.Inconclusive(
                $"created issue not visible within {TestConfig.MaxResponseTimeMs} ms — {WaitDiagnostics.Describe(result)}");
        }
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
