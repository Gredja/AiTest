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
public class GetCollaboratorPermissionTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("28a.1 GET /collaborators/{username}/permission returns 200 OK")]
    public async Task GetCollaboratorPermission_ReturnsOk()
    {
        var response = await Get<PermissionModelResponse>(GitHubEndpoints.RepoCollaboratorPermission,
            [.. TestRepoParam(), .. UsernameParam(TestConfig.GitHubTestUsername)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("28a.2 Repo owner has admin permission and admin role")]
    public async Task GetCollaboratorPermission_OwnerIsAdmin()
    {
        var response = await Get<PermissionModelResponse>(GitHubEndpoints.RepoCollaboratorPermission,
            [.. TestRepoParam(), .. UsernameParam(TestConfig.GitHubTestUsername)]);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Permission.Should().Be(GitHubEndpoints.PermissionAdmin);
        response.Data.RoleName.Should().Be(GitHubEndpoints.PermissionAdmin);
    }

    [Test]
    [Category("Negative")]
    [Description("28a.3 Non-existent user returns 404")]
    public async Task GetCollaboratorPermission_NonExistentUser_ReturnsNotFound()
    {
        var response = await Get<PermissionModelResponse>(GitHubEndpoints.RepoCollaboratorPermission,
            [.. TestRepoParam(), .. UsernameParam(GitHubEndpoints.NonExistentUser)]);

        response.ShouldHaveError(HttpStatusCode.NotFound,
            $"{GitHubEndpoints.NonExistentUser} is not a user");
    }

    [Test]
    [Category("Negative")]
    [Description("28a.4 Non-existent repo returns 404")]
    public async Task GetCollaboratorPermission_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<PermissionModelResponse>(GitHubEndpoints.RepoCollaboratorPermission,
            [.. RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName),
             .. UsernameParam(TestConfig.GitHubTestUsername)]);

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
