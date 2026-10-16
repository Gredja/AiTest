using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using RestSharp;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace Api.GitHub.Collaborators;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetCollaboratorsTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("28.1 GET /collaborators returns 200 OK")]
    public async Task GetCollaborators_ReturnsOk()
    {
        var response = await Get<List<CollaboratorModelResponse>>(
            GitHubEndpoints.RepoCollaborators, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("28.2 Response matches expected contract")]
    public async Task GetCollaborators_ResponseMatchesContract()
    {
        var response = await Get<List<CollaboratorModelResponse>>(
            GitHubEndpoints.RepoCollaborators, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ForEach(collaborator => collaborator.ShouldHaveValidContract());
    }

    [Test]
    [Category("Regression")]
    [Description("28.3 Sandbox owner entry has admin role and all five permission flags")]
    public async Task GetCollaborators_HasValidFields()
    {
        var response = await Get<List<CollaboratorModelResponse>>(
            GitHubEndpoints.RepoCollaborators, TestRepoParam());

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.ForEach(collaborator => collaborator.ShouldHaveValidFields());

        var owner = response.Data.FirstOrDefault(user => user.Login == TestConfig.GitHubTestUsername);
        owner.Should().NotBeNull("the repo owner must be listed as a collaborator");
        owner!.RoleName.Should().Be(GitHubEndpoints.PermissionAdmin);
        owner.Permissions.Admin.Should().BeTrue();
        owner.Permissions.Maintain.Should().BeTrue();
        owner.Permissions.Push.Should().BeTrue();
        owner.Permissions.Triage.Should().BeTrue();
        owner.Permissions.Pull.Should().BeTrue();
    }

    [Test]
    [Category("Negative")]
    [Description("28.4 Non-existent repo returns 404")]
    public async Task GetCollaborators_NonExistentRepo_ReturnsNotFound()
    {
        var response = await Get<List<CollaboratorModelResponse>>(
            GitHubEndpoints.RepoCollaborators,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("28.5 Invalid token returns 401")]
    public async Task GetCollaborators_InvalidToken_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoCollaborators, Method.Get);
        request.AddHeader(AuthorizationHeader, InvalidAuthorization);
        AddParams(request, TestRepoParam());

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.BadCredentials);
    }
}
