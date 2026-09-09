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

namespace Api.JsonPlaceholder.Tests;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class UpdatePostTests : JsonPlaceholderRequestHelper
{
    private static readonly PostModel PutBody = new() { Id = 1, UserId = 1, Title = "Updated Title", Body = "Updated Body" };
    private static readonly PostModel PatchBody = new() { Title = "Patched Title" };

    [Test]
    [Category("HealthCheck")]
    [Description("5.1 PUT returns 200 OK")]
    public async Task UpdatePost_Put_ReturnsOk()
    {
        var response = await Put<PostModel, PostModel>(JsonPlaceholderEndpoints.PostsById, PutBody,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("5.2 PUT response has valid fields")]
    public async Task UpdatePost_Put_HasValidFields()
    {
        var response = await Put<PostModel, PostModel>(JsonPlaceholderEndpoints.PostsById, PutBody,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("HealthCheck")]
    [Description("5.3 PATCH returns 200 OK")]
    public async Task UpdatePost_Patch_ReturnsOk()
    {
        var response = await Patch<PostModel, PostModel>(JsonPlaceholderEndpoints.PostsById, PatchBody,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.4 PATCH response contains updated title")]
    public async Task UpdatePost_Patch_ReturnsUpdatedTitle()
    {
        var response = await Patch<PostModel, PostModel>(JsonPlaceholderEndpoints.PostsById, PatchBody,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.Data!.Title.Should().Be("Patched Title");
    }
}
