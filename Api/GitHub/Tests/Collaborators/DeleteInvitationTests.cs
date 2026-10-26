using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Collaborators;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class DeleteInvitationTests : GitHubTestBase
{
    private long _nonExistentId;

    [OneTimeSetUp]
    public async Task ResolveNonExistentInvitationId()
    {
        var pending = await Get<List<InvitationModelResponse>>(GitHubEndpoints.RepoInvitations, TestRepoParam());
        pending.ShouldHaveStatusCode(HttpStatusCode.OK);

        var maxId = pending.Data!.Select(invitation => invitation.Id).DefaultIfEmpty().Max();
        _nonExistentId = maxId + GitHubEndpoints.NonExistentIdOffset;
    }

    [Test]
    [Category("Negative")]
    [Description("28d.1 Non-existent invitation id returns 404")]
    public async Task DeleteInvitation_NonExistentId_ReturnsNotFound()
    {
        var response = await Delete<object>(GitHubEndpoints.RepoInvitationById,
            [.. TestRepoParam(), .. InvitationIdParam(_nonExistentId)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("28d.2 DELETE without auth returns 401")]
    public async Task DeleteInvitation_NoAuth_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoInvitationById, Method.Delete);
        AddParams(request, [.. TestRepoParam(), .. InvitationIdParam(_nonExistentId)]);

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("28d.3 Invalid token returns 401")]
    public async Task DeleteInvitation_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoInvitationById, Method.Delete);
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        AddParams(request, [.. TestRepoParam(), .. InvitationIdParam(_nonExistentId)]);

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }

    [Test]
    [Category("Negative")]
    [Description("28d.4 Non-existent repo returns 404")]
    public async Task DeleteInvitation_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Delete<object>(GitHubEndpoints.RepoInvitationById,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. InvitationIdParam(_nonExistentId)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
