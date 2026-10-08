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
public class CreatePullRequestTests : GitHubTestBase
{

    private readonly List<(int Number, string HeadBranch, string BaseBranch)> _createdPullRequests = [];
    private readonly List<string> _createdBranchNames = [];

    private const int TitleRandomLength = 8;

    [Test]
    [Category("HealthCheck")]
    [Description("21.1 POST /pulls creates a PR and returns 201")]
    public async Task CreatePullRequest_ReturnsCreated()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        pullNumber.Should().BeGreaterThan(0, "created PR must get a number");
    }

    [Test]
    [Category("Regression")]
    [Description("21.2 Response echoes title and starts open")]
    public async Task CreatePullRequest_EchoesTitleAndIsOpen()
    {
        var title = $"Scratch {DataGenerator.RandomString(TitleRandomLength)}";
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(title);

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var readBack = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(pullNumber));

        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.Title.Should().Be(title, "PR must echo the requested title");
        readBack.Data.State.Should().Be(GitHubEndpoints.StateOpen, "fresh PR must be open");
    }

    [Test]
    [Category("Smoke")]
    [Description("21.3 Content-Type is application/json")]
    public async Task CreatePullRequest_ContentTypeIsJson()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var readBack = await Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById,
            PullParams(pullNumber));

        readBack.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("21.4 POST without auth returns 401")]
    public async Task CreatePullRequest_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoPullRequests, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddJsonBody(new CreatePullRequestModelRequest
        {
            Title = "probe",
            Head = GitHubEndpoints.DefaultBranch,
            Base = GitHubEndpoints.DefaultBranch
        });
        AddParams(request, TestRepoParam());

        var response = await Client.ExecuteAsync<PullRequestModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("21.5 Invalid token returns 401")]
    public async Task CreatePullRequest_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoPullRequests, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        request.AddJsonBody(new CreatePullRequestModelRequest
        {
            Title = "probe",
            Head = GitHubEndpoints.DefaultBranch,
            Base = GitHubEndpoints.DefaultBranch
        });
        AddParams(request, TestRepoParam());

        var response = await Client.ExecuteAsync<PullRequestModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("21.6 Missing head returns 422")]
    public async Task CreatePullRequest_MissingHead_Returns422()
    {
        var response = await Post<Dictionary<string, object>, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequests,
            new Dictionary<string, object>
            {
                [JsonFields.Title] = "probe",
                [JsonFields.Body] = "probe",
                [JsonFields.Base] = GitHubEndpoints.DefaultBranch
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("21.7 head equals base returns 422")]
    public async Task CreatePullRequest_HeadEqualsBase_Returns422()
    {
        var response = await Post<CreatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequests,
            new CreatePullRequestModelRequest
            {
                Title = "probe",
                Head = GitHubEndpoints.DefaultBranch,
                Base = GitHubEndpoints.DefaultBranch
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("21.8 Duplicate PR for same head/base returns 422")]
    public async Task CreatePullRequest_Duplicate_Returns422()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");

        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var duplicate = await Post<CreatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequests,
            new CreatePullRequestModelRequest
            {
                Title = $"Scratch {DataGenerator.RandomString(TitleRandomLength)}",
                Head = headBranch,
                Base = baseBranch
            },
            TestRepoParam());

        duplicate.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("21.9 Head branch with no commits vs base returns 422")]
    public async Task CreatePullRequest_NoCommits_Returns422()
    {
        var branchName = $"audit-{DataGenerator.RandomString(8)}";
        var mainBranch = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);
        mainBranch.ShouldHaveStatusCode(HttpStatusCode.OK);

        var branch = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{RefsHeadsPrefix}{branchName}", Sha = mainBranch.Data!.Commit.Sha },
            TestRepoParam());

        _createdBranchNames.Add(branchName);

        branch.ShouldHaveStatusCode(HttpStatusCode.Created);

        var response = await Post<CreatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequests,
            new CreatePullRequestModelRequest
            {
                Title = "probe",
                Head = branchName,
                Base = GitHubEndpoints.DefaultBranch
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("21.10 Non-existent repo returns 404")]
    public async Task CreatePullRequest_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Post<CreatePullRequestModelRequest, PullRequestModelResponse>(
            GitHubEndpoints.RepoPullRequests,
            new CreatePullRequestModelRequest
            {
                Title = "probe",
                Head = GitHubEndpoints.DefaultBranch,
                Base = GitHubEndpoints.DefaultBranch
            },
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var (number, headBranch, baseBranch) in _createdPullRequests)
        {
            await CleanupPullRequestAsync(number, headBranch, baseBranch);
        }

        foreach (var branchName in _createdBranchNames)
        {
            await CleanupGitRefAsync(branchName);
        }
    }
    [Test]
    [Category("Performance")]
    [Description("21.11 Created PR becomes visible within max response time")]
    public async Task CreatePullRequest_RecordVisibleWithinTimeLimit()
    {
        var (pullNumber, headBranch, baseBranch) = await CreateScratchPullRequestAsync(
            $"Scratch {DataGenerator.RandomString(TitleRandomLength)}");
        _createdPullRequests.Add((pullNumber, headBranch, baseBranch));

        var result = await WaitHelper.WaitUntilAsync(
            () => Get<PullRequestModelResponse>(GitHubEndpoints.RepoPullRequestById, PullParams(pullNumber)),
            response => response.StatusCode == HttpStatusCode.OK && response.Data is not null,
            timeout: TimeSpan.FromMilliseconds(TestConfig.MaxResponseTimeMs));

        result.IsSuccess.Should().BeTrue(
            $"created PR should be visible within {TestConfig.MaxResponseTimeMs} ms;" +
            $" waited {result.Elapsed}, attempts {result.Attempts}," +
            $" last status {result.LastValue?.StatusCode}");
    }
}
