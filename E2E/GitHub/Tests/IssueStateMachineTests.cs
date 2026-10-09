using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class IssueStateMachineTests : GitHubE2ETestBase
{
    private const int TitleRandomLength = 8;

    private readonly List<int> _createdIssueNumbers = [];

    [Test]
    [Category("Regression")]
    [Description("E2E-8 Issue state machine: open → closed → reopen with list filters at each step")]
    public async Task IssueStateMachine_Open_Close_Reopen_VerifyFilters()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues,
            new CreateIssueModelRequest { Title = $"State {DataGenerator.RandomString(TitleRandomLength)}" },
            TestRepoParam());
        if (created.Data is not null)
        {
            _createdIssueNumbers.Add(created.Data.Number);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        var issueNumber = created.Data!.Number;
        var openAfterCreate = await WaitHelper.WaitUntilAsync(
            () => Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
                [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateOpen)]),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data!.Any(issue => issue.Number == issueNumber));
        openAfterCreate.IsSuccess.Should().BeTrue(
            $"created issue #{issueNumber} must appear in ?state=open;" +
            $" elapsed {openAfterCreate.Elapsed}, attempts {openAfterCreate.Attempts}," +
            $" last status {openAfterCreate.LastValue?.StatusCode}");

        var closedListAfterCreate = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateClosed)]);
        closedListAfterCreate.ShouldHaveStatusCode(HttpStatusCode.OK);
        closedListAfterCreate.Data.Should().NotContain(issue => issue.Number == issueNumber,
            "fresh issue must be absent from ?state=closed");

        var closed = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed },
            IssueParams(issueNumber));
        closed.ShouldHaveStatusCode(HttpStatusCode.OK);

        var afterClose = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            IssueParams(issueNumber));
        afterClose.ShouldHaveStatusCode(HttpStatusCode.OK);
        afterClose.Data!.State.Should().Be(GitHubEndpoints.StateClosed, "PATCH state=closed must persist");

        var goneFromOpen = await WaitHelper.WaitUntilAsync(
            () => Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
                [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateOpen)]),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data!.All(issue => issue.Number != issueNumber));
        goneFromOpen.IsSuccess.Should().BeTrue(
            $"closed issue #{issueNumber} must leave ?state=open;" +
            $" elapsed {goneFromOpen.Elapsed}, attempts {goneFromOpen.Attempts}," +
            $" last status {goneFromOpen.LastValue?.StatusCode}");

        var visibleInClosed = await WaitHelper.WaitUntilAsync(
            () => Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
                [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateClosed)]),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data!.Any(issue => issue.Number == issueNumber));
        visibleInClosed.IsSuccess.Should().BeTrue(
            $"closed issue #{issueNumber} must appear in ?state=closed;" +
            $" elapsed {visibleInClosed.Elapsed}, attempts {visibleInClosed.Attempts}," +
            $" last status {visibleInClosed.LastValue?.StatusCode}");

        var reopened = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new UpdateIssueModelRequest { State = GitHubEndpoints.StateOpen },
            IssueParams(issueNumber));
        reopened.ShouldHaveStatusCode(HttpStatusCode.OK);

        var afterReopen = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            IssueParams(issueNumber));
        afterReopen.ShouldHaveStatusCode(HttpStatusCode.OK);
        afterReopen.Data!.State.Should().Be(GitHubEndpoints.StateOpen, "reopen must persist — OB §19");

        var backInOpen = await WaitHelper.WaitUntilAsync(
            () => Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
                [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateOpen)]),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data!.Any(issue => issue.Number == issueNumber));
        backInOpen.IsSuccess.Should().BeTrue(
            $"reopened issue #{issueNumber} must return to ?state=open;" +
            $" elapsed {backInOpen.Elapsed}, attempts {backInOpen.Attempts}," +
            $" last status {backInOpen.LastValue?.StatusCode}");

        var goneFromClosed = await WaitHelper.WaitUntilAsync(
            () => Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
                [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateClosed)]),
            response => response.StatusCode == HttpStatusCode.OK
                && response.Data!.All(issue => issue.Number != issueNumber));
        goneFromClosed.IsSuccess.Should().BeTrue(
            $"reopened issue #{issueNumber} must leave ?state=closed;" +
            $" elapsed {goneFromClosed.Elapsed}, attempts {goneFromClosed.Attempts}," +
            $" last status {goneFromClosed.LastValue?.StatusCode}");
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
