using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using System.Net;
using FluentAssertions;
using RestSharp;
using AllureAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class CreateIssueNegativeTests : GitHubE2ETestBase
{
    private const string MissingTitleErrorMessage = "Invalid request.\n\n\"title\" wasn't supplied.";
    private const string LabelsNullErrorMessage = "Invalid request.\n\nFor 'properties/labels', nil is not an array.";

    [Test]
    [Category("Negative")]
    [Description("E2E-3 POST without title returns 422 naming the missing field")]
    public async Task CreateIssue_MissingTitle_ReturnsUnprocessableEntity()
    {
        var body = new Dictionary<string, object> { ["body"] = "Body without title" };

        var response = await Post<Dictionary<string, object>, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, body, TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.UnprocessableEntity, MissingTitleErrorMessage);
    }

    [Test]
    [Category("Negative")]
    [Description("E2E-4 POST with labels null returns 422 naming the invalid field")]
    public async Task CreateIssue_LabelsNull_ReturnsUnprocessableEntity()
    {
        var body = new Dictionary<string, object?> { ["title"] = "Probe", ["labels"] = null };

        var response = await Post<Dictionary<string, object?>, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, body, TestRepoParam());

        response.ShouldHaveError(HttpStatusCode.UnprocessableEntity, LabelsNullErrorMessage);
    }

    [Test]
    [Category("Negative")]
    [Description("E2E-5 POST without auth returns 401 with documented message")]
    public async Task CreateIssue_NoAuthorization_ReturnsUnauthorized()
    {
        var request = new RestRequest(GitHubEndpoints.RepoIssues, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddJsonBody(new Dictionary<string, object> { ["title"] = "Probe", ["body"] = "Probe" });

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveError(HttpStatusCode.Unauthorized, GitHubErrors.RequiresAuthentication);
    }

    [Test]
    [Category("Negative")]
    [Description("E2E-6 POST to non-existent repo returns 404 with documented message")]
    public async Task CreateIssue_NonExistentRepo_ReturnsNotFound()
    {
        var body = new Dictionary<string, object> { ["title"] = "Probe" };

        var response = await Post<Dictionary<string, object>, IssueModelResponse>(
            GitHubEndpoints.RepoIssues, body,
            RepoParam(GitHubEndpoints.NonExistentUser, GitHubEndpoints.NonExistentRepoName));

        response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound);
    }
}
