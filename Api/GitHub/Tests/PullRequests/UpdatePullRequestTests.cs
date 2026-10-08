using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.PullRequests;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class UpdatePullRequestTests : GitHubTestBase
{

    private readonly List<(int Number, string HeadBranch, string BaseBranch)> _createdPullRequests = [];

    private const int TitleRandomLength = 8;
    private const int ExistingPullNumber = 1;

    [Test]
    [Category("Smoke")]
    [Description("22.1 PATCH title returns 200 and persists")]
    public async Task UpdatePullRequest_PatchTitle_Persists()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var newTitle = $"Patched {DataGenerator.RandomString(TitleRandomLength)}";
        var patchRequest = new UpdatePullRequestModelRequest { Title = newTitle };
        var patched = await Patch<UpdatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequestById,
            patchRequest,
            PullParams(pullNumber));

        patched.ShouldHaveStatusCode(HttpStatusCode.OK);
        patched.Data!.ShouldMatchRequest(patchRequest);

        var readBack = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(pullNumber));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.Title.Should().Be(newTitle, "PATCH must persist the new title");
    }

    [Test]
    [Category("Regression")]
    [Description("22.2 PATCH state=closed persists via read-back")]
    public async Task UpdatePullRequest_ClosePersists()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var closeRequest = new UpdatePullRequestModelRequest { State = GitHubEndpoints.StateClosed };

        var closed = await Patch<UpdatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequestById,
            closeRequest,
            PullParams(pullNumber));

        closed.ShouldHaveStatusCode(HttpStatusCode.OK);
        closed.Data!.ShouldMatchRequest(closeRequest);

        var readBack = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(pullNumber));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.State.Should().Be(GitHubEndpoints.StateClosed, "closed state must persist");
    }

    [Test]
    [Category("Smoke")]
    [Description("22.3 Content-Type is application/json")]
    public async Task UpdatePullRequest_ContentTypeIsJson()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var patched = await Patch<UpdatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequestById,
            new UpdatePullRequestModelRequest { State = GitHubEndpoints.StateClosed },
            PullParams(pullNumber));

        patched.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("22.4 PATCH without auth returns 401")]
    public async Task UpdatePullRequest_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoPullRequestById, Method.Patch)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddJsonBody(new UpdatePullRequestModelRequest { State = GitHubEndpoints.StateClosed });
        AddParams(request, PullParams(ExistingPullNumber));

        var response = await Client.ExecuteAsync<PullRequestModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("22.5 Invalid token returns 401")]
    public async Task UpdatePullRequest_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoPullRequestById, Method.Patch)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        request.AddJsonBody(new UpdatePullRequestModelRequest { State = GitHubEndpoints.StateClosed });
        AddParams(request, PullParams(ExistingPullNumber));

        var response = await Client.ExecuteAsync<PullRequestModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("22.6 Non-existent PR returns 404")]
    public async Task UpdatePullRequest_NonExistentPull_ReturnsNotFound()
    {
        var pulls = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        pulls.ShouldHaveStatusCode(HttpStatusCode.OK);
        pulls.Data.Should().NotBeEmpty("repo must keep data — Entry Criteria, documentation/GitHubTestingStructure.md");
        var nonExistentPullNumber = pulls.Data!.Max(pull => pull.Number) + GitHubEndpoints.NonExistentIdOffset;

        var response = await Patch<UpdatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequestById,
            new UpdatePullRequestModelRequest { State = GitHubEndpoints.StateClosed },
            PullParams(nonExistentPullNumber));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("22.7 Non-existent repo returns 404")]
    public async Task UpdatePullRequest_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Patch<UpdatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequestById,
            new UpdatePullRequestModelRequest { State = GitHubEndpoints.StateClosed },
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. PullRequestNumberParam(1)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var (number, headBranch, baseBranch) in _createdPullRequests)
        {
            await CleanupPullRequestAsync(number, headBranch, baseBranch);
        }
    }
}
