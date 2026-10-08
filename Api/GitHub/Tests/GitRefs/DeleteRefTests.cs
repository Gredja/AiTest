using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using AllureAdapter;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.GitRefs;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class DeleteRefTests : GitHubTestBase
{
    private const string RefPathPrefix = $"{HeadsPrefix}audit-";
    private const string NonExistentRefPath = $"{HeadsPrefix}nonexistent-branch-audit-12345";

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
    [Description("26.1 DELETE /git/refs/{ref} removes a branch and returns 204")]
    public async Task DeleteRef_ReturnsNoContent()
    {
        var refPath = $"{RefPathPrefix}{DataGenerator.RandomString(8)}";
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{RefsHeadsPrefix}{refPath[HeadsPrefix.Length..]}", Sha = _baseSha }, TestRepoParam());
        _createdRefs.Add(refPath);
        created.ShouldHaveStatusCode(HttpStatusCode.Created);

        var deleted = await Delete<object>(
            GitHubEndpoints.RepoGitRefById, [.. TestRepoParam(), .. GitRefParam(refPath)]);

        deleted.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Test]
    [Category("Regression")]
    [Description("26.2 Second delete of the same ref returns 422 (ref is gone)")]
    public async Task DeleteRef_Twice_Returns422()
    {
        var refPath = $"{RefPathPrefix}{DataGenerator.RandomString(8)}";
        var created = await Post<CreateGitRefModelRequest, GitRefModelResponse>(
            GitHubEndpoints.RepoGitRefs,
            new CreateGitRefModelRequest { Ref = $"{RefsHeadsPrefix}{refPath[HeadsPrefix.Length..]}", Sha = _baseSha }, TestRepoParam());
        _createdRefs.Add(refPath);
        created.ShouldHaveStatusCode(HttpStatusCode.Created);

        var first = await Delete<object>(
            GitHubEndpoints.RepoGitRefById, [.. TestRepoParam(), .. GitRefParam(refPath)]);
        first.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        var second = await Delete<object>(
            GitHubEndpoints.RepoGitRefById, [.. TestRepoParam(), .. GitRefParam(refPath)]);
        second.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("26.3 Non-existent ref returns 422")]
    public async Task DeleteRef_NonExistentRef_Returns422()
    {
        var response = await Delete<object>(
            GitHubEndpoints.RepoGitRefById, [.. TestRepoParam(), .. GitRefParam(NonExistentRefPath)]);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("26.4 Default branch deletion returns 422")]
    public async Task DeleteRef_DefaultBranch_Returns422()
    {
        var response = await Delete<object>(
            GitHubEndpoints.RepoGitRefById,
            [.. TestRepoParam(), .. GitRefParam($"{HeadsPrefix}{GitHubEndpoints.DefaultBranch}")]);

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("26.5 Non-existent repo returns 404")]
    public async Task DeleteRef_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Delete<object>(
            GitHubEndpoints.RepoGitRefById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. GitRefParam(RefPathPrefix)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("26.6 DELETE without auth returns 401")]
    public async Task DeleteRef_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoGitRefById, Method.Delete);
        AddParams(request, [.. TestRepoParam(), .. GitRefParam(NonExistentRefPath)]);

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("26.7 Invalid token returns 401")]
    public async Task DeleteRef_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoGitRefById, Method.Delete);
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        AddParams(request, [.. TestRepoParam(), .. GitRefParam(NonExistentRefPath)]);

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
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
