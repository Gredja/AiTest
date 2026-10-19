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
public class CreateGitTreeTests : GitHubTestBase
{
    private const string ProbePath = "phase1-invalid-sha-probe.txt";

    private string _baseTreeSha = string.Empty;

    [OneTimeSetUp]
    public async Task ResolveBaseTreeSha() => _baseTreeSha = await ResolveDefaultTreeShaAsync();

    [Test]
    [Category("Negative")]
    [Description("26b.1 Invalid entry sha returns 422")]
    public async Task CreateGitTree_InvalidSha_Returns422()
    {
        var response = await Post<CreateGitTreeModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitTrees,
            new CreateGitTreeModelRequest
            {
                BaseTree = _baseTreeSha,
                Tree =
                [
                    new GitTreeEntry
                    {
                        Path = ProbePath,
                        Mode = FileMode644,
                        Type = BlobType,
                        Sha = GitHubEndpoints.NonExistentSha
                    }
                ]
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }
}
