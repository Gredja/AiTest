using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using RestSharp;
using AllureAdapter;
using static Core.Helpers.ParamHelper;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderTestData;

namespace Api.JsonPlaceholder.Posts;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class CreatePostTests : JsonPlaceholderRequestHelper
{
    private const int TestUserId = 1;
    private const int TitleRandomLength = 8;
    private const int BodyRandomLength = 16;
    private const string IgnoreMissingFieldsBug = "JsonPlaceholder mock: accepts any body — missing required fields still return 201 — Bug: documentation/Bugs/JsonPlaceholder/JP-001-create-post-accepts-empty-body.md";
    private static readonly PostModelRequest _testPost = new()
    {
        UserId = TestUserId,
        Title = $"Post {DataGenerator.RandomString(TitleRandomLength)}",
        Body = $"Body {DataGenerator.RandomString(BodyRandomLength)}"
    };

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

        response.Data!.Title.Should().Be(_testPost.Title);
        response.Data!.Body.Should().Be(_testPost.Body);
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
    [Ignore("JsonPlaceholder mock: accepts any body, returns 201 even for empty request — Bug: documentation/Bugs/JsonPlaceholder/JP-001-create-post-accepts-empty-body.md")]
    [Category("Negative")]
    [Description("4.7 Empty body returns error")]
    public async Task CreatePost_EmptyBody_ReturnsError()
    {
        var emptyPost = new PostModelRequest();
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, emptyPost);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    private const string SpecialCharsTitle = "Test special chars: <>&\"";
    private const string MalformedJson = "{\"title\":";
    private const string NonObjectJson = "\"just-a-string\"";
    private const string NotANumberUserId = "not-a-number";

    [Test]
    [Category("Smoke")]
    [Description("4.8 Special chars in title are accepted")]
    public async Task CreatePost_SpecialCharsInTitle_ReturnsCreated()
    {
        var specialPost = new PostModelRequest { UserId = TestUserId, Title = SpecialCharsTitle, Body = $"Body {DataGenerator.RandomString(BodyRandomLength)}" };
        var response = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, specialPost);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        response.Data!.Title.Should().Be(SpecialCharsTitle);
    }

    [Test]
    [Ignore(IgnoreMissingFieldsBug)]
    [Category("Negative")]
    [Description("4.9 POST without title returns 400")]
    public async Task CreatePost_MissingTitle_ReturnsBadRequest()
    {
        var post = new Dictionary<string, object> { [BodyKey] = "Body", [UserIdKey] = TestUserId };
        var response = await Post<Dictionary<string, object>, PostModelResponse>(JsonPlaceholderEndpoints.Posts, post);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Ignore(IgnoreMissingFieldsBug)]
    [Category("Negative")]
    [Description("4.10 POST without body returns 400")]
    public async Task CreatePost_MissingBody_ReturnsBadRequest()
    {
        var post = new Dictionary<string, object> { [TitleKey] = "Title", [UserIdKey] = TestUserId };
        var response = await Post<Dictionary<string, object>, PostModelResponse>(JsonPlaceholderEndpoints.Posts, post);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Ignore(IgnoreMissingFieldsBug)]
    [Category("Negative")]
    [Description("4.11 POST without userId returns 400")]
    public async Task CreatePost_MissingUserId_ReturnsBadRequest()
    {
        var post = new Dictionary<string, object> { [TitleKey] = "Title", [BodyKey] = "Body" };
        var response = await Post<Dictionary<string, object>, PostModelResponse>(JsonPlaceholderEndpoints.Posts, post);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: accepts any body — wrong field types still return 201 — Bug: documentation/Bugs/JsonPlaceholder/JP-001-create-post-accepts-empty-body.md")]
    [Category("Negative")]
    [Description("4.12 POST with wrong field types returns 400")]
    public async Task CreatePost_WrongFieldTypes_ReturnsBadRequest()
    {
        var post = new Dictionary<string, object> { [TitleKey] = 123, [BodyKey] = true, [UserIdKey] = NotANumberUserId };
        var response = await Post<Dictionary<string, object>, PostModelResponse>(JsonPlaceholderEndpoints.Posts, post);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Category("Negative")]
    [Description("4.13 POST with malformed JSON returns 500")]
    public async Task CreatePost_MalformedJson_ReturnsInternalServerError()
    {
        var request = new RestRequest(JsonPlaceholderEndpoints.Posts, Method.Post);
        request.AddStringBody(MalformedJson, ContentType.Json);

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);
    }

    [Test]
    [Category("Negative")]
    [Description("4.14 POST with non-object JSON returns 500")]
    public async Task CreatePost_NonObjectJson_ReturnsInternalServerError()
    {
        var request = new RestRequest(JsonPlaceholderEndpoints.Posts, Method.Post);
        request.AddStringBody(NonObjectJson, ContentType.Json);

        var response = await Client.ExecuteAsync(request);

        response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: POST does not persist — GET never returns the created record (GET /posts/{id} → 404), documented mock behavior — see Rules/test-practices.md → Read-after-write visibility (fake-API exception)")]
    [Category("Performance")]
    [Description("4.15 Created post becomes visible within max response time")]
    public async Task CreatePost_RecordVisibleWithinTimeLimit()
    {
        var created = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, _testPost);
        created.ShouldHaveStatusCode(HttpStatusCode.Created);

        var result = await WaitHelper.WaitUntilAsync(
            () => Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById, IdParam(created.Data!.Id)),
            response => response.StatusCode == HttpStatusCode.OK && response.Data is not null,
            timeout: TimeSpan.FromMilliseconds(TestConfig.MaxResponseTimeMs));

        result.IsSuccess.Should().BeTrue(
            $"created post should be visible within {TestConfig.MaxResponseTimeMs} ms;" +
            $" waited {result.Elapsed}, attempts {result.Attempts}," +
            $" last status {result.LastValue?.StatusCode}");
    }
}
