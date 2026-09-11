using NUnit.Framework;
using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Posts;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class CreatePostTests : JsonPlaceholderRequestHelper
{
    private static readonly PostRequest _testPost = new() { UserId = JsonPlaceholderEndpoints.TestUserId, Title = "Test Post", Body = "Test Body" };

    [Test]
    [Category("HealthCheck")]
    [Description("4.1 POST returns 201 Created")]
    public async Task CreatePost_ReturnsCreated()
    {
        var response = await Post<PostRequest, PostModel>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Test]
    [Category("Regression")]
    [Description("4.2 Response has valid fields")]
    public async Task CreatePost_HasValidFields()
    {
        var response = await Post<PostRequest, PostModel>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Smoke")]
    [Description("4.3 Response contains correct data")]
    public async Task CreatePost_ReturnsCorrectData()
    {
        var response = await Post<PostRequest, PostModel>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.Data!.Title.Should().Be("Test Post");
        response.Data!.Body.Should().Be("Test Body");
        response.Data!.UserId.Should().Be(JsonPlaceholderEndpoints.TestUserId);
    }

    [Test]
    [Category("Smoke")]
    [Description("4.4 Response has generated ID")]
    public async Task CreatePost_HasGeneratedId()
    {
        var response = await Post<PostRequest, PostModel>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.Data!.Id.Should().BeGreaterThan(0);
    }
}
