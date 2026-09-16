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
public class UpdatePostTests : JsonPlaceholderRequestHelper
{
    private const int TestPostId = 1;
    private const int TestUserId = 1;
    private static readonly PostRequest _putBody = new() { UserId = TestUserId, Title = "Updated Title", Body = "Updated Body" };
    private static readonly PostRequest _patchBody = new() { Title = "Patched Title" };

    [Test]
    [Category("HealthCheck")]
    [Description("5.1 PUT returns 200 OK")]
    public async Task UpdatePost_Put_ReturnsOk()
    {
        var response = await Put<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _putBody,
            PostIdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("5.2 PUT response has valid fields")]
    public async Task UpdatePost_Put_HasValidFields()
    {
        var response = await Put<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _putBody,
            PostIdParam(TestPostId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("HealthCheck")]
    [Description("5.3 PATCH returns 200 OK")]
    public async Task UpdatePost_Patch_ReturnsOk()
    {
        var response = await Patch<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            PostIdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.4 PATCH response contains updated title")]
    public async Task UpdatePost_Patch_ReturnsUpdatedTitle()
    {
        var response = await Patch<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            PostIdParam(TestPostId));

        response.Data!.Title.Should().Be("Patched Title");
    }

    [Test]
    [Category("Regression")]
    [Description("5.5 PUT response matches request via ShouldMatchRequest")]
    public async Task UpdatePost_Put_ShouldMatchRequest()
    {
        var response = await Put<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _putBody,
            PostIdParam(TestPostId));

        response.Data!.ShouldMatchRequest(_putBody);
    }

    [Test]
    [Category("Regression")]
    [Description("5.6 PUT preserves original ID")]
    public async Task UpdatePost_Put_PreservesOriginalId()
    {
        var response = await Put<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _putBody,
            PostIdParam(TestPostId));

        response.Data!.Id.Should().Be(TestPostId);
    }

    [Test]
    [Category("Negative")]
    [Description("5.7 PUT non-existent ID returns error")]
    public async Task UpdatePost_Put_NonExistentId_ReturnsError()
    {
        var response = await Put<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _putBody,
            PostIdParam(0));

        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: PATCH does not merge with existing data — returns only sent fields + ID, rest is null")]
    [Category("Regression")]
    [Description("5.8 PATCH only changes specified fields")]
    public async Task UpdatePost_Patch_OnlyChangesSpecifiedFields()
    {
        var response = await Patch<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            PostIdParam(TestPostId));

        response.Data!.Title.Should().Be("Patched Title");
        response.Data!.Body.Should().NotBeNull();
    }
}
