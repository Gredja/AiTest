using NUnit.Framework;
using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Tests;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class CreatePostTests : JsonPlaceholderRequestHelper
{
    [Test]
    [Category("HealthCheck")]
    [Description("4.1 POST returns 201 Created")]
    public async Task CreatePost_ReturnsCreated()
    {
        var body = new PostModel { UserId = 1, Title = "Test Post", Body = "Test Body" };
        var response = await Post<PostModel, PostModel>(JsonPlaceholderEndpoints.Posts, body);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Test]
    [Category("Regression")]
    [Description("4.2 Response has valid fields")]
    public async Task CreatePost_HasValidFields()
    {
        var body = new PostModel { UserId = 1, Title = "Test Post", Body = "Test Body" };
        var response = await Post<PostModel, PostModel>(JsonPlaceholderEndpoints.Posts, body);

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Smoke")]
    [Description("4.3 Response contains correct data")]
    public async Task CreatePost_ReturnsCorrectData()
    {
        var body = new PostModel { UserId = 1, Title = "Test Post", Body = "Test Body" };
        var response = await Post<PostModel, PostModel>(JsonPlaceholderEndpoints.Posts, body);

        response.Data!.Title.Should().Be("Test Post");
        response.Data!.Body.Should().Be("Test Body");
        response.Data!.UserId.Should().Be(1);
    }

    [Test]
    [Category("Smoke")]
    [Description("4.4 Response has generated ID")]
    public async Task CreatePost_HasGeneratedId()
    {
        var body = new PostModel { UserId = 1, Title = "Test Post", Body = "Test Body" };
        var response = await Post<PostModel, PostModel>(JsonPlaceholderEndpoints.Posts, body);

        response.Data!.Id.Should().BeGreaterThan(0);
    }
}
