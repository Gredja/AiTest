using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.ParamHelper;

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
}
