using Core.Config;
using Core.Generators.GitHub;
using Core.Helpers;
using Core.Models.GitHub;
using System.Net;
using FluentAssertions;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class IssuesTests : GitHubE2ETestBase
{
    private readonly List<int> _createdIssueNumbers = [];

    private static readonly List<string> _issueLabels = ["bug", "urgent"];

    private const string SpecialCharsTitle = "E2E special <title> & \"quoted\" — Ünïcödé #42";

    private const string SpecialCharsBody =
        "Markdown *body* with <b>html</b>, emoji \U0001F600, pipe | and newline\nsecond line";

    [Test]
    [Category("Regression")]
    [Description("E2E-10.1 Full-field issue create → 201 → content matches request → GET read-back → close in teardown")]
    public async Task Issues_CreateAllFields_ReadBackMatches()
    {
        var request = IssueGenerator.WithTitleBodyLabels("E2E full-field", "E2E full-field body", _issueLabels);
        var created = await CreateIssueTrackedAsync(request, _createdIssueNumbers);
        created.ShouldHaveValidFields();

        // Labels: request List<string> vs response List<Label> — ShouldMatchRequest cannot compare across these shapes (documented exception, Rules/assertions.md)
        created.Title.Should().Be(request.Title);
        created.Body.Should().Be(request.Body);
        created.Labels.Select(label => label.Name).Should().BeEquivalentTo(request.Labels);
        created.State.Should().Be(GitHubEndpoints.StateOpen);

        var readBack = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById, IssueParams(created.Number));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.Number.Should().Be(created.Number);
        readBack.Data.Title.Should().Be(request.Title);
        readBack.Data.Body.Should().Be(request.Body);
        readBack.Data.State.Should().Be(GitHubEndpoints.StateOpen);
        readBack.Data.Labels.Select(label => label.Name).Should().BeEquivalentTo(request.Labels);
    }

    [Test]
    [Category("Regression")]
    [Description("E2E-10.2 PATCH title+body+labels in one call → changed fields persist, state preserved → read-back")]
    public async Task Issues_PatchTitleBodyLabels_ChangedFieldsPersist()
    {
        var created = await CreateIssueTrackedAsync(
            IssueGenerator.CreateRandom("E2E patch source", "E2E patch source body"),
            _createdIssueNumbers);

        var patchRequest = IssueGenerator.WithTitleBodyLabels("E2E patch target", "E2E patch target body", _issueLabels);

        // OB §19 patchable content subset is title/body/labels with no state — CreateIssueModelRequest models exactly that wire shape
        var patched = await Patch<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById, patchRequest, IssueParams(created.Number));
        patched.ShouldHaveStatusCode(HttpStatusCode.OK);
        patched.Data!.Title.Should().Be(patchRequest.Title);
        patched.Data.Body.Should().Be(patchRequest.Body);
        patched.Data.State.Should().Be(GitHubEndpoints.StateOpen, "content PATCH must not touch state (OB §19)");
        patched.Data.Labels.Select(label => label.Name).Should().BeEquivalentTo(patchRequest.Labels);

        var readBack = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById, IssueParams(created.Number));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.Title.Should().Be(patchRequest.Title);
        readBack.Data.Body.Should().Be(patchRequest.Body);
        readBack.Data.State.Should().Be(GitHubEndpoints.StateOpen);
        readBack.Data.Labels.Select(label => label.Name).Should().BeEquivalentTo(patchRequest.Labels);
    }

    [Test]
    [Category("Regression")]
    [Description("E2E-10.3 Close one of two issues → open-list count decrements, sibling unaffected")]
    public async Task Issues_CloseOneOfTwo_OpenListCountDecrements()
    {
        var first = await CreateIssueTrackedAsync(
            IssueGenerator.CreateRandom("E2E count first"), _createdIssueNumbers);
        var second = await CreateIssueTrackedAsync(
            IssueGenerator.CreateRandom("E2E count second"), _createdIssueNumbers);

        await WaitUntilIssueInStateListAsync(first.Number, GitHubEndpoints.StateOpen, isPresent: true);
        await WaitUntilIssueInStateListAsync(second.Number, GitHubEndpoints.StateOpen, isPresent: true);

        var openBefore = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateOpen)]);
        openBefore.ShouldHaveStatusCode(HttpStatusCode.OK);

        await PatchIssueStateAsync(first.Number, GitHubEndpoints.StateClosed);
        await WaitUntilIssueInStateListAsync(first.Number, GitHubEndpoints.StateOpen, isPresent: false);

        var openAfter = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateOpen)]);
        openAfter.ShouldHaveStatusCode(HttpStatusCode.OK);
        openAfter.Data!.Count.Should().Be(openBefore.Data!.Count - 1,
            "closing one of two issues must decrement the open list by exactly one");
        openAfter.Data.Should().Contain(issue => issue.Number == second.Number,
            "the sibling issue must remain in the open list");

        await AssertIssueStateAsync(second.Number, GitHubEndpoints.StateOpen,
            "the sibling issue must stay open");
    }

    [Test]
    [Category("Negative")]
    [Description("E2E-10.4 DELETE issue → 404 (endpoint absent, OB §24) → fallback PATCH close succeeds")]
    public async Task Issues_DeleteEndpointAbsent_FallbackCloseCleans()
    {
        var created = await CreateIssueTrackedAsync(
            IssueGenerator.CreateRandom("E2E delete-absent"), _createdIssueNumbers);

        var deleteResponse = await Delete<object>(GitHubEndpoints.RepoIssueById, IssueParams(created.Number));
        deleteResponse.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);

        await PatchIssueStateAsync(created.Number, GitHubEndpoints.StateClosed);
        await AssertIssueStateAsync(created.Number, GitHubEndpoints.StateClosed,
            "cleanup must fall back to close — DELETE /issues/{n} is absent (OB §24)");
    }

    [Test]
    [Category("Regression")]
    [Description("E2E-10.5 Rapid create→close→create cycle → both issues persist with correct states")]
    public async Task Issues_RapidCreateCloseCreate_BothPersisted()
    {
        var first = await CreateIssueTrackedAsync(
            IssueGenerator.CreateRandom("E2E rapid first"), _createdIssueNumbers);
        await PatchIssueStateAsync(first.Number, GitHubEndpoints.StateClosed);
        await AssertIssueStateAsync(first.Number, GitHubEndpoints.StateClosed,
            "the first issue must close before the second create");

        var second = await CreateIssueTrackedAsync(
            IssueGenerator.CreateRandom("E2E rapid second"), _createdIssueNumbers);
        await AssertIssueStateAsync(second.Number, GitHubEndpoints.StateOpen,
            "the second issue must open while the first stays closed");
        await AssertIssueStateAsync(first.Number, GitHubEndpoints.StateClosed,
            "the first issue must stay closed after the second create");
    }

    [Test]
    [Category("Regression")]
    [Description("E2E-10.6 Special-char title/body → 201 → GET read-back fidelity → close in teardown")]
    public async Task Issues_CreateSpecialChars_ReadBackMatches()
    {
        var request = IssueGenerator.CreateWithTitle(SpecialCharsTitle);
        request.Body = SpecialCharsBody;

        var created = await CreateIssueTrackedAsync(request, _createdIssueNumbers);
        created.Title.Should().Be(SpecialCharsTitle);
        created.Body.Should().Be(SpecialCharsBody);

        var readBack = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById, IssueParams(created.Number));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.Title.Should().Be(SpecialCharsTitle);
        readBack.Data.Body.Should().Be(SpecialCharsBody);
        readBack.Data.State.Should().Be(GitHubEndpoints.StateOpen);
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
