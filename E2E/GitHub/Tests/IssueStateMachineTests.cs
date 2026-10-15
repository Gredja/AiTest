using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;
using static Core.Helpers.TestDataConstants;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class IssueStateMachineTests : GitHubE2ETestBase
{
    private readonly List<int> _createdIssueNumbers = [];

    [Test]
    [Category("Regression")]
    [Description("E2E-8 Issue state machine: open → closed → reopen with list filters at each step")]
    public async Task IssueStateMachine_Open_Close_Reopen_VerifyFilters()
    {
        var created = await CreateIssueTrackedAsync(
            new CreateIssueModelRequest
            {
                Title = $"State {DataGenerator.RandomString(TitleRandomLength)}",
                Body = $"State machine body {DataGenerator.RandomString(BodyRandomLength)}"
            },
            _createdIssueNumbers);
        var issueNumber = created.Number;

        await WaitUntilIssueInStateListAsync(issueNumber, GitHubEndpoints.StateOpen, present: true);

        var closedList = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateClosed)]);
        closedList.ShouldHaveStatusCode(HttpStatusCode.OK);
        closedList.Data.Should().NotContain(issue => issue.Number == issueNumber,
            "fresh issue must be absent from ?state=closed");

        await PatchIssueStateAsync(issueNumber, GitHubEndpoints.StateClosed);
        await AssertIssueStateAsync(issueNumber, GitHubEndpoints.StateClosed, "PATCH state=closed must persist");
        await WaitUntilIssueInStateListAsync(issueNumber, GitHubEndpoints.StateOpen, present: false);
        await WaitUntilIssueInStateListAsync(issueNumber, GitHubEndpoints.StateClosed, present: true);

        await PatchIssueStateAsync(issueNumber, GitHubEndpoints.StateOpen);
        await AssertIssueStateAsync(issueNumber, GitHubEndpoints.StateOpen, "reopen must persist — OB §19");
        await WaitUntilIssueInStateListAsync(issueNumber, GitHubEndpoints.StateOpen, present: true);
        await WaitUntilIssueInStateListAsync(issueNumber, GitHubEndpoints.StateClosed, present: false);
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
