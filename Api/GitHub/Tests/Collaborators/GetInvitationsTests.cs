using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Collaborators;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetInvitationsTests : GitHubTestBase
{
    [Test]
    [Category("Smoke")]
    [Description("28c.1 GET /invitations returns 200 with an array body")]
    public async Task GetInvitations_ReturnsOk()
    {
        var response = await Get<List<InvitationModelResponse>>(GitHubEndpoints.RepoInvitations, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("ContractCheck")]
    [Description("28c.2 Pending invitation entries match the invitation contract")]
    public async Task GetInvitations_ResponseMatchesContract()
    {
        var response = await Get<List<InvitationModelResponse>>(GitHubEndpoints.RepoInvitations, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ForEach(invitation => invitation.ShouldHaveValidContract());
    }

    [Test]
    [Category("Regression")]
    [Description("28c.3 Invitation ids are positive and unique")]
    public async Task GetInvitations_IdsArePositiveAndUnique()
    {
        var response = await Get<List<InvitationModelResponse>>(GitHubEndpoints.RepoInvitations, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Should().OnlyContain(invitation => invitation.Id > 0);
        response.Data.Select(invitation => invitation.Id).Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Negative")]
    [Description("28c.4 Non-existent repo returns 404")]
    public async Task GetInvitations_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<InvitationModelResponse>>(GitHubEndpoints.RepoInvitations,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Smoke")]
    [Description("28c.5 Content-Type is application/json")]
    public async Task GetInvitations_ContentTypeIsJson()
    {
        var response = await Get<List<InvitationModelResponse>>(GitHubEndpoints.RepoInvitations, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }
}
