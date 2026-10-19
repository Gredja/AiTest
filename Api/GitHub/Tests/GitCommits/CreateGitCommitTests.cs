using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using AllureAdapter;

namespace Api.GitHub.GitCommits;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class CreateGitCommitTests : GitHubTestBase
{
    private const string ProbeMessage = "phase1 invalid parent probe";

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
                Message = ProbeMessage,
                Tree = _baseTreeSha,
                Parents = [GitHubEndpoints.NonExistentSha]
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }
}
