using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using AllureAdapter;
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
}
