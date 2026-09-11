using NUnit.Framework;
using RestSharp;
using Core.Models;
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
    private static readonly PostRequest _putBody = new() { UserId = JsonPlaceholderEndpoints.TestUserId, Title = "Updated Title", Body = "Updated Body" };
    private static readonly PostRequest _patchBody = new() { Title = "Patched Title" };

    [Test]
    [Category("HealthCheck")]
    [Description("5.1 PUT returns 200 OK")]
    public async Task UpdatePost_Put_ReturnsOk()
    {
        var response = await Put<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _putBody,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("5.2 PUT response has valid fields")]
    public async Task UpdatePost_Put_HasValidFields()
    {
        var response = await Put<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _putBody,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("HealthCheck")]
    [Description("5.3 PATCH returns 200 OK")]
    public async Task UpdatePost_Patch_ReturnsOk()
    {
        var response = await Patch<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.4 PATCH response contains updated title")]
    public async Task UpdatePost_Patch_ReturnsUpdatedTitle()
    {
        var response = await Patch<PostRequest, PostModel>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.Data!.Title.Should().Be("Patched Title");
    }
}
