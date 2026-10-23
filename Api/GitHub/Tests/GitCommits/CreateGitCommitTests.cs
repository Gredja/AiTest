using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;

namespace Api.GitHub.GitCommits;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class CreateGitCommitTests : GitHubTestBase
{
    // Ceiling: 5 active negatives — distinct useful failure modes are exhausted.
    // parents is wire-optional (POST without it → 201, probed 2026-10-22), so no
    // MissingParents negative exists; [RequiredField] on Parents is the client contract
    private const string ProbeCommitMessage = "phase1 invalid-sha probe message";

    private string _baseTreeSha = string.Empty;

    [OneTimeSetUp]
    public async Task ResolveBaseTreeSha() => _baseTreeSha = await ResolveDefaultTreeShaAsync();

    [Test]
    [Category("Negative")]
    [Description("27.3 Invalid parent sha returns 422")]
    public async Task CreateGitCommit_InvalidParentSha_Returns422()
    {
        var response = await Post<CreateGitCommitModelRequest, GitCommitModelResponse>(
            GitHubEndpoints.RepoGitCommits,
            new CreateGitCommitModelRequest
            {
                Message = ProbeCommitMessage,
                Tree = _baseTreeSha,
                Parents = [GitHubEndpoints.NonExistentSha]
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("27.4 POST without message returns 422")]
    public async Task CreateGitCommit_MissingMessage_Returns422()
    {
        var response = await Post<Dictionary<string, object>, GitCommitModelResponse>(
            GitHubEndpoints.RepoGitCommits,
            new Dictionary<string, object> { [JsonFields.Tree] = _baseTreeSha },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("27.5 POST without tree returns 422")]
    public async Task CreateGitCommit_MissingTree_Returns422()
    {
        var response = await Post<Dictionary<string, object>, GitCommitModelResponse>(
            GitHubEndpoints.RepoGitCommits,
            new Dictionary<string, object> { [JsonFields.Message] = ProbeCommitMessage },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("27.6 Invalid tree sha returns 422")]
    public async Task CreateGitCommit_InvalidTreeSha_Returns422()
    {
        var response = await Post<CreateGitCommitModelRequest, GitCommitModelResponse>(
            GitHubEndpoints.RepoGitCommits,
            new CreateGitCommitModelRequest
            {
                Message = ProbeCommitMessage,
                Tree = GitHubEndpoints.NonExistentSha,
                Parents = []
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("27.7 Message with wrong type returns 422")]
    public async Task CreateGitCommit_MessageWrongType_Returns422()
    {
        var response = await Post<Dictionary<string, object>, GitCommitModelResponse>(
            GitHubEndpoints.RepoGitCommits,
            new Dictionary<string, object>
            {
                [JsonFields.Message] = 123,
                [JsonFields.Tree] = _baseTreeSha
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }
}
