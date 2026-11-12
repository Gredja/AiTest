using Core.Models.GitHub;
using Core.Generators;
using Core.Generators.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;
using static Core.Helpers.TestDataConstants;

namespace Api.GitHub.GitRefs;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class CreateRefTests : GitHubTestBase
{
    private const string BranchPrefix = $"{GitHubEndpoints.RefsHeadsPrefix}audit-";

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
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            GitRefGenerator.Create(refName, _baseSha), TestRepoParam());

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdRefs.Add(refName);
        created.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("25.2 Response matches request")]
    public async Task CreateRef_MatchesRequest()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var createRequest = GitRefGenerator.Create(refName, _baseSha);
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs, createRequest, TestRepoParam());

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdRefs.Add(refName);
        created.Data.Should().NotBeNull();
        created.Data!.ShouldMatchRequest(GitRefGenerator.CreateRefOnly(refName));
        created.Data.Object.Sha.Should().Be(_baseSha, "request Sha maps to nested object.sha — documented ShouldMatchRequest exception");
    }

    [Test]
    [Category("Regression")]
    [Description("25.3 object.type is commit")]
    public async Task CreateRef_ObjectTypeIsCommit()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            GitRefGenerator.Create(refName, _baseSha), TestRepoParam());

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdRefs.Add(refName);
        created.Data!.Object.Type.Should().Be(GitHubEndpoints.ObjectTypeCommit);
        created.Data!.Object.Sha.ShouldHaveValidSha();
    }

    [Test]
    [Category("Negative")]
    [Description("25.4 POST without auth returns 401")]
    public async Task CreateRef_NoAuth_ReturnsUnauthorized()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var request = new RestRequest(GitHubEndpoints.RepoGitRefs, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddJsonBody(GitRefGenerator.Create(refName, _baseSha));
        AddParams(request, TestRepoParam());

        var response = await Client.ExecuteAsync<GitRefModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("25.5 Invalid token returns 401")]
    public async Task CreateRef_InvalidToken_ReturnsUnauthorized()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var request = new RestRequest(GitHubEndpoints.RepoGitRefs, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        request.AddJsonBody(GitRefGenerator.Create(refName, _baseSha));
        AddParams(request, TestRepoParam());

        var response = await Client.ExecuteAsync<GitRefModelResponse>(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("25.6 Non-existent sha returns 422")]
    public async Task CreateRef_MissingSha_Returns422()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var response = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            GitRefGenerator.Create(refName, GitHubEndpoints.NonExistentSha),
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
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var first = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            GitRefGenerator.Create(refName, _baseSha), TestRepoParam());

        first.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdRefs.Add(refName);

        var second = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            GitRefGenerator.Create(refName, _baseSha), TestRepoParam());

        second.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("25.9 Non-existent repo returns 404")]
    public async Task CreateRef_NonExistentRepo_ReturnsNotFound()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var response = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            GitRefGenerator.Create(refName, _baseSha),
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Performance")]
    [Description("25.10 Created ref becomes visible within max response time")]
    public async Task CreateRef_RecordVisibleWithinTimeLimit()
    {
        var refName = $"{BranchPrefix}{DataGenerator.RandomString(RandomNameLength)}";
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            GitRefGenerator.Create(refName, _baseSha), TestRepoParam());

        created.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdRefs.Add(refName);

        var refPath = $"{GitHubEndpoints.HeadsPrefix}{refName[GitHubEndpoints.RefsHeadsPrefix.Length..]}";
        var result = await WaitHelper.WaitUntilAsync(
            () => Get<GitRefModelResponse>(GitHubEndpoints.RepoGitRefByName,
                [.. TestRepoParam(), .. GitRefParam(refPath)]),
            response => response.StatusCode == HttpStatusCode.OK && response.Data is not null,
            timeout: TimeSpan.FromMilliseconds(TestConfig.MaxResponseTimeMs));

        result.IsSuccess.Should().BeTrue(
            $"created ref not visible within {TestConfig.MaxResponseTimeMs} ms — {WaitDiagnosticsHelper.Describe(result)}");
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
