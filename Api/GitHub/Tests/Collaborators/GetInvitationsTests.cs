using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;

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
}
