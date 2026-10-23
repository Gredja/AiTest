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
public class CreateInvitationTests : GitHubTestBase
{
    private readonly List<long> _createdInvitationIds = [];

    [OneTimeSetUp]
    public async Task OneTimeSetUp() => await NormalizePendingCollaboratorInvitationsAsync();

    [Test]
    [Category("HealthCheck")]
    [Description("28b.1 PUT /collaborators/{username} with push returns 201 Created")]
    public async Task CreateInvitation_ReturnsCreated()
    {
        var invite = await Put<CreateCollaboratorModelRequest, InvitationModelResponse>(
            GitHubEndpoints.RepoCollaboratorByUsername,
            new CreateCollaboratorModelRequest { Permission = GitHubEndpoints.PermissionPush },
            [.. TestRepoParam(), .. UsernameParam(TestConfig.GitHubCollaboratorUser)]);
        if (invite.Data is not null)
        {
            _createdInvitationIds.Add(invite.Data.Id);
        }

        invite.ShouldHaveStatusCode(HttpStatusCode.Created);
        invite.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Negative")]
    [Description("28b.2 Permission pull on a public repo returns 422")]
    public async Task CreateInvitation_PullOnPublicRepo_Returns422()
    {
        var invite = await Put<CreateCollaboratorModelRequest, InvitationModelResponse>(
            GitHubEndpoints.RepoCollaboratorByUsername,
            new CreateCollaboratorModelRequest { Permission = GitHubEndpoints.PermissionPull },
            [.. TestRepoParam(), .. UsernameParam(TestConfig.GitHubCollaboratorUser)]);

        invite.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    [Category("Negative")]
    [Description("28b.3 Non-existent user returns 404")]
    public async Task CreateInvitation_NonExistentUser_ReturnsNotFound()
    {
        var invite = await Put<CreateCollaboratorModelRequest, InvitationModelResponse>(
            GitHubEndpoints.RepoCollaboratorByUsername,
            new CreateCollaboratorModelRequest { Permission = GitHubEndpoints.PermissionPush },
            [.. TestRepoParam(), .. UsernameParam(GitHubEndpoints.NonExistentUser)]);

        invite.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var invitationId in _createdInvitationIds)
        {
            await CleanupInvitationAsync(invitationId);
        }
    }
}
