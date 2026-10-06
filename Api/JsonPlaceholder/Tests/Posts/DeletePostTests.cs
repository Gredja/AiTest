using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using RestSharp;
using TestAdapter;
using static Core.Helpers.ParamHelper;

namespace Api.JsonPlaceholder.Posts;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class DeletePostTests : JsonPlaceholderRequestHelper
{
    private const int TestPostId = 1;
    private const string MalformedJson = "{\"title\":";
    private const string NonNumericId = "abc";

    [Test]
    [Category("HealthCheck")]
    [Description("6.1 DELETE returns 200 OK")]
    public async Task DeletePost_ReturnsOk()
    {
        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            IdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("6.2 DELETE response is empty object")]
    public async Task DeletePost_ReturnsEmptyObject()
    {
        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            IdParam(TestPostId));

        response.Data.Should().NotBeNull();
    }

    [Test]
    [Ignore("JsonPlaceholder mock: DELETE returns 200 but does not actually delete — GET still returns the resource — Bug: documentation/Bugs/JsonPlaceholder/JP-002-delete-post-does-not-actually-delete.md")]
    [Category("Regression")]
    [Description("6.3 Deleted post returns 404 on subsequent GET")]
    public async Task DeletePost_DeletedPostReturnsNotFound()
    {
        var deleteResponse = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            IdParam(TestPostId));

        var getResponse = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            IdParam(TestPostId));

        getResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: DELETE on any ID returns 200, even non-existent — Bug: documentation/Bugs/JsonPlaceholder/JP-003-delete-post-returns-200-for-non-existent-id.md")]
    [Category("Negative")]
    [Description("6.4 DELETE with ID 0 returns error")]
    public async Task DeletePost_ZeroId_ReturnsError()
    {
        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            IdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: DELETE on any ID returns 200, even non-existent — Bug: documentation/Bugs/JsonPlaceholder/JP-003-delete-post-returns-200-for-non-existent-id.md")]
    [Category("Negative")]
    [Description("6.5 DELETE with non-existent ID returns 404")]
    public async Task DeletePost_NonExistentId_ReturnsNotFound()
    {
        var allPosts = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);
        var nonExistentId = allPosts.Data!.Max(post => post.Id) + 1;

        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            IdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: DELETE on any ID returns 200, even non-existent — Bug: documentation/Bugs/JsonPlaceholder/JP-003-delete-post-returns-200-for-non-existent-id.md")]
    [Category("Negative")]
    [Description("6.6 DELETE with negative ID returns 404")]
    public async Task DeletePost_NegativeId_ReturnsNotFound()
    {
        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            IdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: DELETE on any ID returns 200, even non-existent — Bug: documentation/Bugs/JsonPlaceholder/JP-003-delete-post-returns-200-for-non-existent-id.md")]
    [Category("Negative")]
    [Description("6.7 DELETE with non-numeric ID returns 404")]
    public async Task DeletePost_InvalidSegment_ReturnsNotFound()
    {
        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            [UrlSegment("id", NonNumericId)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("6.8 DELETE with malformed JSON body returns 500")]
    public async Task DeletePost_MalformedJson_ReturnsInternalServerError()
    {
        var request = new RestRequest(JsonPlaceholderEndpoints.PostsById, Method.Delete);
        request.AddUrlSegment("id", TestPostId);
        request.AddStringBody(MalformedJson, ContentType.Json);

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);
    }

    [Test]
    [Category("Negative")]
    [Description("6.9 DELETE on collection endpoint returns 404")]
    public async Task DeletePost_CollectionEndpoint_ReturnsNotFound()
    {
        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.Posts);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
