using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;

namespace Api.GitHub.GitCommits;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class CreateGitTreeTests : GitHubTestBase
{
    // Ceiling: 5 active negatives — distinct useful failure modes are exhausted.
    // base_tree is wire-optional (POST without it → 201, probed 2026-10-22), so no
    // MissingBaseTree negative exists even though the request model marks it required
    private const string ProbeEntryPath = "phase1-probe.txt";

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
                        Path = ProbeEntryPath,
                        Mode = FileMode644,
                        Type = BlobType,
                        Sha = GitHubEndpoints.NonExistentSha
                    }
                ]
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("26b.2 POST without tree array returns 422")]
    public async Task CreateGitTree_MissingTree_Returns422()
    {
        var response = await Post<Dictionary<string, object>, GitShaModelResponse>(
            GitHubEndpoints.RepoGitTrees,
            new Dictionary<string, object> { [JsonFields.BaseTree] = _baseTreeSha },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("26b.3 Entry without sha or content returns 422")]
    public async Task CreateGitTree_EntryWithoutShaOrContent_Returns422()
    {
        var response = await Post<Dictionary<string, object>, GitShaModelResponse>(
            GitHubEndpoints.RepoGitTrees,
            new Dictionary<string, object>
            {
                [JsonFields.BaseTree] = _baseTreeSha,
                [JsonFields.Tree] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        [JsonFields.Path] = ProbeEntryPath,
                        [JsonFields.Mode] = FileMode644,
                        [JsonFields.Type] = BlobType
                    }
                }
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("26b.4 Entry without path returns 422")]
    public async Task CreateGitTree_EntryWithoutPath_Returns422()
    {
        var response = await Post<Dictionary<string, object>, GitShaModelResponse>(
            GitHubEndpoints.RepoGitTrees,
            new Dictionary<string, object>
            {
                [JsonFields.BaseTree] = _baseTreeSha,
                [JsonFields.Tree] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        [JsonFields.Mode] = FileMode644,
                        [JsonFields.Type] = BlobType,
                        [JsonFields.Content] = "phase1 probe content"
                    }
                }
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("26b.5 Entry without mode returns 422")]
    public async Task CreateGitTree_EntryWithoutMode_Returns422()
    {
        var response = await Post<Dictionary<string, object>, GitShaModelResponse>(
            GitHubEndpoints.RepoGitTrees,
            new Dictionary<string, object>
            {
                [JsonFields.BaseTree] = _baseTreeSha,
                [JsonFields.Tree] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        [JsonFields.Path] = ProbeEntryPath,
                        [JsonFields.Type] = BlobType,
                        [JsonFields.Content] = "phase1 probe content"
                    }
                }
            },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }
}
