using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Posts;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class DeletePostTests : JsonPlaceholderRequestHelper
{
    private const int TestPostId = 1;
    [Test]
    [Category("HealthCheck")]
    [Description("6.1 DELETE returns 200 OK")]
    public async Task DeletePost_ReturnsOk()
    {
        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("6.2 DELETE response is empty object")]
    public async Task DeletePost_ReturnsEmptyObject()
    {
        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        response.Data.Should().NotBeNull();
    }

    [Test]
    [Ignore("JsonPlaceholder mock: DELETE returns 200 but does not actually delete — GET still returns the resource — Bug: documentation/Bugs/JsonPlaceholder/JP-002-delete-post-does-not-actually-delete.md")]
    [Category("Regression")]
    [Description("6.3 Deleted post returns 404 on subsequent GET")]
    public async Task DeletePost_DeletedPostReturnsNotFound()
    {
        var deleteResponse = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        var getResponse = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        getResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: DELETE on any ID returns 200, even non-existent — Bug: documentation/Bugs/JsonPlaceholder/JP-003-delete-post-returns-200-for-non-existent-id.md")]
    [Category("Negative")]
    [Description("6.4 DELETE non-existent ID returns error")]
    public async Task DeletePost_NonExistentId_ReturnsError()
    {
        var response = await Delete<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(0));

        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }
}
