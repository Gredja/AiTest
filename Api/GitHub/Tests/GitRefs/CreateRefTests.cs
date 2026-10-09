using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.GitRefs;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class CreateRefTests : GitHubTestBase
{
    private const string BranchPrefix = $"{RefsHeadsPrefix}audit-";
    private const string MissingSha = "0000000000000000000000000000000000000000";

    private readonly List<string> _createdRefs = [];
    private string _baseSha = string.Empty;

    [OneTimeSetUp]
    public async Task ResolveBaseSha()
    {
        var branch = await Get<BranchModelResponse>(GitHubEndpoints.RepoBranchByName,
            [.. TestRepoParam(), .. BranchNameParam(GitHubEndpoints.DefaultBranch)]);
        branch.ShouldHaveStatusCode(HttpStatusCode.OK);
        _baseSha = branch.Data!.Commit.Sha;
    }

    [Test]
    [Category("HealthCheck")]
    [Description("25.1 POST /git/refs creates a branch and returns 201")]
    public async Task CreateRef_ReturnsCreated()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(8)}";
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = refName, Sha = _baseSha }, TestRepoParam());
        _createdRefs.Add(refName);

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();
    }

    [Test]
    [Category("ContractCheck")]
    [Description("25.2 Response matches request")]
    public async Task CreateRef_MatchesRequest()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(8)}";
        var createRequest = new CreateGitRefModelRequest { Ref = refName, Sha = _baseSha };
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs, createRequest, TestRepoParam());
        _createdRefs.Add(refName);

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data.Should().NotBeNull();
        created.Data!.Ref.Should().Be(refName, "response must echo the created ref");
        created.Data.Object.Sha.Should().Be(_baseSha, "response must point at the requested sha");
    }

    [Test]
    [Category("Regression")]
    [Description("25.3 object.type is commit")]
    public async Task CreateRef_ObjectTypeIsCommit()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(8)}";
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = refName, Sha = _baseSha }, TestRepoParam());
        _createdRefs.Add(refName);

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        created.Data!.Object.Type.Should().Be("commit");
        created.Data!.Object.Sha.ShouldHaveValidSha();
    }

    [Test]
    [Category("Negative")]
    [Description("25.4 POST without auth returns 401")]
    public async Task CreateRef_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoGitRefs, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddJsonBody(new CreateGitRefModelRequest
        {
            Ref = $"{BranchPrefix}{DataGenerator.RandomString(8)}",
            Sha = _baseSha
        });
        AddParams(request, TestRepoParam());

        var response = await Client.ExecuteAsync<GitRefModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("25.5 Invalid token returns 401")]
    public async Task CreateRef_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoGitRefs, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        request.AddJsonBody(new CreateGitRefModelRequest
        {
            Ref = $"{BranchPrefix}{DataGenerator.RandomString(8)}",
            Sha = _baseSha
        });
        AddParams(request, TestRepoParam());

        var response = await Client.ExecuteAsync<GitRefModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("25.6 Non-existent sha returns 422")]
    public async Task CreateRef_MissingSha_Returns422()
    {
        var response = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{BranchPrefix}{DataGenerator.RandomString(8)}", Sha = MissingSha },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("25.7 POST without required fields returns 422")]
    public async Task CreateRef_EmptyBody_Returns422()
    {
        var response = await Post<Dictionary<string, object>, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs, new Dictionary<string, object>(), TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("25.8 Duplicate ref returns 422")]
    public async Task CreateRef_DuplicateRef_Returns422()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(8)}";
        var first = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = refName, Sha = _baseSha }, TestRepoParam());
        _createdRefs.Add(refName);

        first.ShouldHaveStatusCode(HttpStatusCode.Created);

        var second = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = refName, Sha = _baseSha }, TestRepoParam());

        second.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("25.9 Non-existent repo returns 404")]
    public async Task CreateRef_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{BranchPrefix}{DataGenerator.RandomString(8)}", Sha = _baseSha },
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Performance")]
    [Description("25.10 Created ref becomes visible within max response time")]
    public async Task CreateRef_RecordVisibleWithinTimeLimit()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(8)}";
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = refName, Sha = _baseSha }, TestRepoParam());
        _createdRefs.Add(refName);

        created.ShouldHaveStatusCode(HttpStatusCode.Created);

        var refPath = $"{HeadsPrefix}{refName[RefsHeadsPrefix.Length..]}";
        var result = await WaitHelper.WaitUntilAsync(
            () => Get<GitRefModelResponse>(GitHubEndpoints.RepoGitRefByName,
                [.. TestRepoParam(), .. GitRefParam(refPath)]),
            response => response.StatusCode == HttpStatusCode.OK && response.Data is not null,
            timeout: TimeSpan.FromMilliseconds(TestConfig.MaxResponseTimeMs));

        result.IsSuccess.Should().BeTrue(
            $"created ref should be visible within {TestConfig.MaxResponseTimeMs} ms;" +
            $" waited {result.Elapsed}, attempts {result.Attempts}," +
            $" last status {result.LastValue?.StatusCode}");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var refPath in _createdRefs)
        {
            await CleanupGitRefAsync(refPath);
        }
    }
}
