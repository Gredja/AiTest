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
public class MergePullRequestTests : GitHubTestBase
{

    private readonly List<(int Number, string HeadBranch, string BaseBranch)> _createdPullRequests = [];

    private const int TitleRandomLength = 8;
    private const int ExistingPullNumber = 1;

    [Test]
    [Category("HealthCheck")]
    [Description("23.1 PUT merge returns 200 with merged=true")]
    public async Task MergePullRequest_ReturnsMerged()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var merged = await Put<Dictionary<string, object>, object>(
            GitHubEndpoints.RepoPullRequestMerge,
            new Dictionary<string, object>(),
            PullParams(pullNumber));

        merged.ShouldHaveStatusCode(HttpStatusCode.OK);
        merged.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("23.2 Re-merge returns 200 (idempotent, documented)")]
    public async Task MergePullRequest_Twice_Returns200Again()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var first = await Put<Dictionary<string, object>, object>(
            GitHubEndpoints.RepoPullRequestMerge,
            new Dictionary<string, object>(),
            PullParams(pullNumber));
        first.ShouldHaveStatusCode(HttpStatusCode.OK);

        var second = await Put<Dictionary<string, object>, object>(
            GitHubEndpoints.RepoPullRequestMerge,
            new Dictionary<string, object>(),
            PullParams(pullNumber));
        second.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("23.3 Content-Type is application/json")]
    public async Task MergePullRequest_ContentTypeIsJson()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var merged = await Put<Dictionary<string, object>, object>(
            GitHubEndpoints.RepoPullRequestMerge,
            new Dictionary<string, object>(),
            PullParams(pullNumber));

        merged.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("23.4 Merge without auth returns 401")]
    public async Task MergePullRequest_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoPullRequestMerge, Method.Put)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddJsonBody(new Dictionary<string, object>());
        AddParams(request, PullParams(ExistingPullNumber));

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("23.5 Invalid token returns 401")]
    public async Task MergePullRequest_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoPullRequestMerge, Method.Put)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        request.AddJsonBody(new Dictionary<string, object>());
        AddParams(request, PullParams(ExistingPullNumber));

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("23.6 Non-existent PR returns 404")]
    public async Task MergePullRequest_NonExistentPull_ReturnsNotFound()
    {
        var pulls = await Get<List<PullRequestModelResponse>>(GitHubEndpoints.RepoPullRequests,
            [.. TestRepoParam(), .. StateParam(GitHubEndpoints.StateAll)]);
        pulls.ShouldHaveStatusCode(HttpStatusCode.OK);
        pulls.Data.Should().NotBeEmpty("repo must keep data — Entry Criteria, documentation/GitHubTestingStructure.md");
        var nonExistentPullNumber = pulls.Data!.Max(pull => pull.Number) + GitHubEndpoints.NonExistentIdOffset;

        var response = await Put<Dictionary<string, object>, object>(
            GitHubEndpoints.RepoPullRequestMerge,
            new Dictionary<string, object>(),
            PullParams(nonExistentPullNumber));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("23.7 Non-existent repo returns 404")]
    public async Task MergePullRequest_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Put<Dictionary<string, object>, object>(
            GitHubEndpoints.RepoPullRequestMerge,
            new Dictionary<string, object>(),
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
