using Api.GitHub.Helpers;
using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.GitCommits;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class CreateGitBlobTests : GitBlobTestBase
{
    private const string UnsupportedEncoding = "rot13";

    [Test]
    [Category("HealthCheck")]
    [Description("26c.1 POST /git/blobs returns 201 Created")]
    public async Task CreateGitBlob_ReturnsCreated()
    {
        var response = await Post<CreateGitBlobModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs,
            new CreateGitBlobModelRequest { Content = RandomBlobContent() },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        response.Data!.Sha.ShouldHaveValidSha();
    }

    [Test]
    [Category("Regression")]
    [Description("26c.2 Response sha equals the git blob hash of the content")]
    public async Task CreateGitBlob_ShaMatchesGitBlobHash()
    {
        var content = RandomBlobContent();

        var response = await Post<CreateGitBlobModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs,
            new CreateGitBlobModelRequest { Content = content },
            TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        response.Data!.Sha.Should().Be(ExpectedBlobSha(content));
    }

    [Test]
    [Category("Regression")]
    [Description("26c.3 Unsupported encoding is ignored — raw content is stored")]
    public async Task CreateGitBlob_UnsupportedEncoding_StoresRawContent()
    {
        var content = RandomBlobContent();
        var body = new Dictionary<string, object>
        {
            [JsonFields.Content] = content,
            [JsonFields.Encoding] = UnsupportedEncoding
        };

        var response = await Post<Dictionary<string, object>, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs, body, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        response.Data!.Sha.Should().Be(ExpectedBlobSha(content),
            "encoding is not validated — GitHub stores the raw content string");
    }

    [Test]
    [Category("Negative")]
    [Description("26c.4 POST without content returns 422")]
    public async Task CreateGitBlob_MissingContent_Returns422()
    {
        var response = await Post<Dictionary<string, object>, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs, new Dictionary<string, object>(), TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.UnprocessableEntity, GitHubErrors.ValidationFailed);
    }

    [Test]
    [Category("Negative")]
    [Description("26c.5 Non-string content returns 422")]
    public async Task CreateGitBlob_ContentNotString_Returns422()
    {
        var body = new Dictionary<string, object> { [JsonFields.Content] = 123 };

        var response = await Post<Dictionary<string, object>, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs, body, TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.UnprocessableEntity, GitHubErrors.ContentNotString);
    }

    [Test]
    [Category("Negative")]
    [Description("26c.6 Non-existent repo returns 404")]
    public async Task CreateGitBlob_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Post<CreateGitBlobModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs,
            new CreateGitBlobModelRequest { Content = RandomBlobContent() },
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("26c.7 Non-existent owner returns 404")]
    public async Task CreateGitBlob_NonExistentOwner_ReturnsNotFound()
    {
        var response = await Post<CreateGitBlobModelRequest, GitShaModelResponse>(
            GitHubEndpoints.RepoGitBlobs,
            new CreateGitBlobModelRequest { Content = RandomBlobContent() },
            RepoParam(GitHubEndpoints.NonExistentUser, ParseRepo().Repo));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("26c.8 POST without auth returns 401")]
    public async Task CreateGitBlob_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoGitBlobs, Method.Post);
        request.AddJsonBody(new CreateGitBlobModelRequest { Content = RandomBlobContent() });
        AddParams(request, TestRepoParam());

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("26c.9 Invalid token returns 401")]
    public async Task CreateGitBlob_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoGitBlobs, Method.Post);
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        request.AddJsonBody(new CreateGitBlobModelRequest { Content = RandomBlobContent() });
        AddParams(request, TestRepoParam());

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
