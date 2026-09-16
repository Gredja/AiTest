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
    private const int TestUserId = 1;
    private static readonly PostModelRequest _testPost = new() { UserId = TestUserId, Title = "Test Post", Body = "Test Body" };

    [Test]
    [Category("HealthCheck")]
    [Description("4.1 POST returns 201 Created")]
    public async Task CreatePost_ReturnsCreated()
    {
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Test]
    [Category("Regression")]
    [Description("4.2 Response has valid fields")]
    public async Task CreatePost_HasValidFields()
    {
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Smoke")]
    [Description("4.3 Response contains correct data")]
    public async Task CreatePost_ReturnsCorrectData()
    {
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.Data!.Title.Should().Be("Test Post");
        response.Data!.Body.Should().Be("Test Body");
        response.Data!.UserId.Should().Be(TestUserId);
    }

    [Test]
    [Category("Smoke")]
    [Description("4.4 Response has generated ID")]
    public async Task CreatePost_HasGeneratedId()
    {
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.Data!.Id.Should().BeGreaterThan(0);
    }

    [Test]
    [Category("Regression")]
    [Description("4.5 Response userId matches request")]
    public async Task CreatePost_UserIdMatchesRequest()
    {
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.Data!.UserId.Should().Be(TestUserId);
    }

    [Test]
    [Category("Regression")]
    [Description("4.6 Response matches request via ShouldMatchRequest")]
    public async Task CreatePost_ShouldMatchRequest()
    {
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, _testPost);

        response.Data!.ShouldMatchRequest(_testPost);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: accepts any body, returns 201 even for empty request")]
    [Category("Negative")]
    [Description("4.7 Empty body returns error")]
    public async Task CreatePost_EmptyBody_ReturnsError()
    {
        var emptyPost = new PostModelRequest();
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, emptyPost);

        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    private const string SpecialCharsTitle = "Test special chars: <>&\"";

    [Test]
    [Category("Smoke")]
    [Description("4.8 Special chars in title are accepted")]
    public async Task CreatePost_SpecialCharsInTitle_ReturnsCreated()
    {
        var specialPost = new PostModelRequest { UserId = TestUserId, Title = SpecialCharsTitle, Body = "Body" };
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, specialPost);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        response.Data!.Title.Should().Be(SpecialCharsTitle);
    }
}
