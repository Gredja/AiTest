using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class CollaboratorPermissionTests : GitHubE2ETestBase
{
    private const string InvitePermission = "push";
    private const string PendingPermissionValue = "write";

    private long? _createdInvitationId;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var pending = await Get<List<InvitationModelResponse>>(GitHubEndpoints.RepoInvitations, TestRepoParam());
        pending.ShouldHaveStatusCode(HttpStatusCode.OK);
        foreach (var stale in pending.Data!
                     .Where(item => item.Invitee.Login == TestConfig.GitHubCollaboratorUser))
        {
            await CleanupInvitationAsync(stale.Id);
        }
    }

    [Test]
    [Category("Regression")]
    [Description("E2E-5 Collaborators: verify owner access → invite → verify pending → revoke")]
    public async Task CollaboratorAccess_Invite_VerifyPending_Revoke()
    {
        var collaboratorUser = TestConfig.GitHubCollaboratorUser;

        var collaborators = await Get<List<CollaboratorModelResponse>>(GitHubEndpoints.RepoCollaborators,
            TestRepoParam());
        collaborators.ShouldHaveStatusCode(HttpStatusCode.OK);
        var owner = collaborators.Data!.FirstOrDefault(user => user.Login == TestConfig.GitHubTestUsername);
        owner.Should().NotBeNull("repo owner must be in the collaborators list");
        owner!.Permissions.Admin.Should().BeTrue("owner must hold admin permission");
        owner.RoleName.Should().Be("admin", "owner role must be admin");

        var ownerPermission = await Get<PermissionModelResponse>(GitHubEndpoints.RepoCollaboratorPermission,
            [.. TestRepoParam(), .. UsernameParam(TestConfig.GitHubTestUsername)]);
        ownerPermission.ShouldHaveStatusCode(HttpStatusCode.OK);
        ownerPermission.Data!.Permission.Should().Be("admin", "owner must have admin permission");

        var invite = await Put<CreateCollaboratorModelRequest, InvitationModelResponse>(
            GitHubEndpoints.RepoCollaboratorByUsername,
            new CreateCollaboratorModelRequest { Permission = InvitePermission },
            [.. TestRepoParam(), .. UsernameParam(collaboratorUser)]);
        if (invite.Data is not null)
        {
            _createdInvitationId = invite.Data.Id;
        }

        invite.ShouldHaveStatusCode(HttpStatusCode.Created);
        invite.Data!.Permissions.Should().Be(PendingPermissionValue, "invitation must carry the requested write access");

        var pending = await Get<List<InvitationModelResponse>>(GitHubEndpoints.RepoInvitations, TestRepoParam());
        pending.ShouldHaveStatusCode(HttpStatusCode.OK);
        var invitation = pending.Data!.FirstOrDefault(item => item.Invitee.Login == collaboratorUser);
        invitation.Should().NotBeNull("pending invitation must be listed after invite");
        invitation!.Permissions.Should().Be(PendingPermissionValue);
        invitation.Expired.Should().BeFalse("fresh invitation must not be expired");

        var afterInvite = await Get<List<CollaboratorModelResponse>>(GitHubEndpoints.RepoCollaborators,
            TestRepoParam());
        afterInvite.ShouldHaveStatusCode(HttpStatusCode.OK);
        afterInvite.Data.Should().NotContain(user => user.Login == collaboratorUser,
            "pending invitee must not be a collaborator until the invitation is accepted");

        var inviteePermission = await Get<PermissionModelResponse>(GitHubEndpoints.RepoCollaboratorPermission,
            [.. TestRepoParam(), .. UsernameParam(collaboratorUser)]);
        inviteePermission.ShouldHaveStatusCode(HttpStatusCode.OK);
        inviteePermission.Data!.Permission.Should().Be("read",
            "pending invitation must not grant more than the public base access");

        var revoke = await Delete<object>(GitHubEndpoints.RepoInvitationById,
            [.. TestRepoParam(), .. InvitationIdParam(_createdInvitationId!.Value)]);
        revoke.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        var afterRevoke = await Get<List<InvitationModelResponse>>(GitHubEndpoints.RepoInvitations, TestRepoParam());
        afterRevoke.ShouldHaveStatusCode(HttpStatusCode.OK);
        afterRevoke.Data.Should().NotContain(item => item.Invitee.Login == collaboratorUser,
            "revoked invitation must disappear from the list");

        var finalCollaborators = await Get<List<CollaboratorModelResponse>>(GitHubEndpoints.RepoCollaborators,
            TestRepoParam());
        finalCollaborators.ShouldHaveStatusCode(HttpStatusCode.OK);
        finalCollaborators.Data.Should().NotContain(user => user.Login == collaboratorUser,
            "revoke must leave the collaborators list unchanged");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await CleanupInvitationAsync(_createdInvitationId);
    }
}
