using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Tests;

[TestFixture]
[AllureNUnit]
public class UpdatePostTests : JsonPlaceholderRequestHelper
{
    [Test]
    [Description("5.1 PUT returns 200 OK")]
    public async Task UpdatePost_Put_ReturnsOk()
    {
        var body = new PostModel { Id = 1, UserId = 1, Title = "Updated Title", Body = "Updated Body" };
        var response = await Put<PostModel, PostModel>(JsonPlaceholderEndpoints.PostsById, body,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Description("5.2 PUT response has valid fields")]
    public async Task UpdatePost_Put_HasValidFields()
    {
        var body = new PostModel { Id = 1, UserId = 1, Title = "Updated Title", Body = "Updated Body" };
        var response = await Put<PostModel, PostModel>(JsonPlaceholderEndpoints.PostsById, body,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Description("5.3 PATCH returns 200 OK")]
    public async Task UpdatePost_Patch_ReturnsOk()
    {
        var body = new PostModel { Title = "Patched Title" };
        var response = await Patch<PostModel, PostModel>(JsonPlaceholderEndpoints.PostsById, body,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Description("5.4 PATCH response contains updated title")]
    public async Task UpdatePost_Patch_ReturnsUpdatedTitle()
    {
        var body = new PostModel { Title = "Patched Title" };
        var response = await Patch<PostModel, PostModel>(JsonPlaceholderEndpoints.PostsById, body,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.Data!.Title.Should().Be("Patched Title");
    }

    private static List<RequestDictionaryModel> PostIdParam(int id)
    {
        return new List<RequestDictionaryModel>
        {
            new() { Type = "UrlSegment", Key = "id", Value = id }
        };
    }
}
