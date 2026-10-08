using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Issues;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class UpdateIssueTests : GitHubTestBase
{
    private const int TargetIssueNumber = 5;
    private const int TitleRandomLength = 8;
    private const string InvalidStateValue = "bogus";

    private static readonly CreateIssueModelRequest _createRequest = new()
    {
        Title = $"Patch target {DataGenerator.RandomString(TitleRandomLength)}",
        Body = $"Body {DataGenerator.RandomString(TitleRandomLength)}"
    };

    private readonly List<int> _createdIssueNumbers = [];

    [Test]
    [Category("Smoke")]
    [Description("19.1 PATCH title returns 200 and echoes the new title")]
    public async Task UpdateIssue_PatchTitle_ReturnsUpdatedTitle()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, _createRequest, TestRepoParam());

        if (created.Data is not null)
        {
            _createdIssueNumbers.Add(created.Data.Number);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();

        var patchRequest = new UpdateIssueModelRequest
        {
            Title = $"Patched {DataGenerator.RandomString(TitleRandomLength)}"
        };
        var patched = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById, patchRequest,
            IssueParams(created.Data!.Number));

        patched.ShouldHaveStatusCode(HttpStatusCode.OK);
        patched.Data.Should().NotBeNull();
        patched.Data!.ShouldMatchRequest(patchRequest);

        var readBack = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            IssueParams(created.Data.Number));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.Title.Should().Be(patchRequest.Title, "PATCH must persist the new title");
    }

    [Test]
    [Category("Regression")]
    [Description("19.2 PATCH state=closed persists via read-back")]
    public async Task UpdateIssue_ClosePersists()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, _createRequest, TestRepoParam());

        if (created.Data is not null)
        {
            _createdIssueNumbers.Add(created.Data.Number);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();

        var closeRequest = new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed };
        var closed = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById, closeRequest, IssueParams(created.Data!.Number));

        closed.ShouldHaveStatusCode(HttpStatusCode.OK);
        closed.Data!.State.Should().Be(GitHubEndpoints.StateClosed);

        var readBack = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            IssueParams(created.Data.Number));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.State.Should().Be(GitHubEndpoints.StateClosed, "closed state must persist");
    }

    [Test]
    [Category("Regression")]
    [Description("19.3 Close then reopen persists via read-back")]
    public async Task UpdateIssue_ReopenPersists()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, _createRequest, TestRepoParam());

        if (created.Data is not null)
        {
            _createdIssueNumbers.Add(created.Data.Number);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();

        var closed = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed },
            IssueParams(created.Data!.Number));
        closed.ShouldHaveStatusCode(HttpStatusCode.OK);
        closed.Data!.State.Should().Be(GitHubEndpoints.StateClosed);

        var reopened = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new UpdateIssueModelRequest { State = GitHubEndpoints.StateOpen },
            IssueParams(created.Data.Number));
        reopened.ShouldHaveStatusCode(HttpStatusCode.OK);
        reopened.Data!.State.Should().Be(GitHubEndpoints.StateOpen);

        var readBack = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            IssueParams(created.Data.Number));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.State.Should().Be(GitHubEndpoints.StateOpen, "reopened state must persist");
    }

    [Test]
    [Category("Smoke")]
    [Description("19.4 Content-Type is application/json")]
    public async Task UpdateIssue_ContentTypeIsJson()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, _createRequest, TestRepoParam());

        if (created.Data is not null)
        {
            _createdIssueNumbers.Add(created.Data.Number);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);

        var patched = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed },
            IssueParams(created.Data!.Number));

        patched.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("19.5 PATCH without auth returns 401")]
    public async Task UpdateIssue_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoIssueById, Method.Patch)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddJsonBody(new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed });
        AddParams(request, IssueParams(TargetIssueNumber));

        var response = await Client.ExecuteAsync<IssueModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("19.6 Invalid token returns 401")]
    public async Task UpdateIssue_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoIssueById, Method.Patch)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        request.AddJsonBody(new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed });
        AddParams(request, IssueParams(TargetIssueNumber));

        var response = await Client.ExecuteAsync<IssueModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("19.7 PATCH with labels=null returns 422")]
    public async Task UpdateIssue_LabelsNull_Returns422()
    {
        var response = await Patch<Dictionary<string, object?>, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new Dictionary<string, object?> { [JsonFields.Labels] = null },
            IssueParams(TargetIssueNumber));

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("19.8 Non-existent issue returns 404")]
    public async Task UpdateIssue_NonExistentIssue_ReturnsNotFound()
    {
        var issues = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        issues.ShouldHaveStatusCode(HttpStatusCode.OK);
        var nonExistentIssueNumber = issues.Data!.Max(issue => issue.Number) + GitHubEndpoints.NonExistentIdOffset;

        var response = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed },
            IssueParams(nonExistentIssueNumber));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("19.9 Non-existent repo returns 404")]
    public async Task UpdateIssue_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new UpdateIssueModelRequest { State = GitHubEndpoints.StateClosed },
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("19.10 Unrecognized state value is silently ignored (documented)")]
    public async Task UpdateIssue_UnrecognizedState_IsIgnored()
    {
        var created = await Post<CreateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, _createRequest, TestRepoParam());

        if (created.Data is not null)
        {
            _createdIssueNumbers.Add(created.Data.Number);
        }

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();

        var bogus = await Patch<UpdateIssueModelRequest, IssueModelResponse>(
            GitHubEndpoints.RepoIssueById,
            new UpdateIssueModelRequest { State = InvalidStateValue },
            IssueParams(created.Data!.Number));

        bogus.ShouldHaveStatusCode(HttpStatusCode.OK);
        bogus.Data!.State.Should().Be(GitHubEndpoints.StateOpen);

        var readBack = await Get<IssueModelResponse>(GitHubEndpoints.RepoIssueById,
            IssueParams(created.Data.Number));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.State.Should().Be(GitHubEndpoints.StateOpen,
            "unrecognized state must be ignored — OB §19");
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
