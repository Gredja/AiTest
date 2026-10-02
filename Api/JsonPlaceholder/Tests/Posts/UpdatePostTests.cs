using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using RestSharp;
using TestAdapter;
using static Core.Helpers.ParamHelper;

namespace Api.JsonPlaceholder.Posts;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class UpdatePostTests : JsonPlaceholderRequestHelper
{
    private const int TestPostId = 1;
    private const int TestUserId = 1;
    private const string MalformedJson = "{\"title\":";
    private const string NonNumericId = "abc";
    private static readonly PostModelRequest _putBody = new() { UserId = TestUserId, Title = "Updated Title", Body = "Updated Body" };
    private static readonly PostModelRequest _patchBody = new() { Title = "Patched Title" };

    [Test]
    [Category("HealthCheck")]
    [Description("5.1 PUT returns 200 OK")]
    public async Task UpdatePost_Put_ReturnsOk()
    {
        var response = await Put<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _putBody,
            IdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("5.2 PUT response has valid fields")]
    public async Task UpdatePost_Put_HasValidFields()
    {
        var response = await Put<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _putBody,
            IdParam(TestPostId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("HealthCheck")]
    [Description("5.3 PATCH returns 200 OK")]
    public async Task UpdatePost_Patch_ReturnsOk()
    {
        var response = await Patch<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            IdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("5.4 PATCH response contains updated title")]
    public async Task UpdatePost_Patch_ReturnsUpdatedTitle()
    {
        var response = await Patch<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            IdParam(TestPostId));

        response.Data!.Title.Should().Be("Patched Title");
    }

    [Test]
    [Category("Regression")]
    [Description("5.5 PUT response matches request via ShouldMatchRequest")]
    public async Task UpdatePost_Put_ShouldMatchRequest()
    {
        var response = await Put<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _putBody,
            IdParam(TestPostId));

        response.Data!.ShouldMatchRequest(_putBody);
    }

    [Test]
    [Category("Regression")]
    [Description("5.6 PUT preserves original ID")]
    public async Task UpdatePost_Put_PreservesOriginalId()
    {
        var response = await Put<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _putBody,
            IdParam(TestPostId));

        response.Data!.Id.Should().Be(TestPostId);
    }

    [Test]
    [Category("Negative")]
    [Description("5.7 PUT with ID 0 returns error")]
    public async Task UpdatePost_Put_ZeroId_ReturnsError()
    {
        var response = await Put<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _putBody,
            IdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: PATCH does not merge with existing data — returns only sent fields + ID, rest is null — Bug: documentation/Bugs/JsonPlaceholder/JP-004-patch-post-does-not-merge-with-existing-data.md")]
    [Category("Regression")]
    [Description("5.8 PATCH only changes specified fields")]
    public async Task UpdatePost_Patch_OnlyChangesSpecifiedFields()
    {
        var response = await Patch<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            IdParam(TestPostId));

        response.Data!.Title.Should().Be("Patched Title");
        response.Data!.Body.Should().NotBeNull();
    }

    [Test]
    [Category("Negative")]
    [Description("5.9 PUT with negative ID returns 500")]
    public async Task UpdatePost_Put_NegativeId_ReturnsInternalServerError()
    {
        var response = await Put<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _putBody,
            IdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);
    }

    [Test]
    [Category("Negative")]
    [Description("5.10 PUT with non-existent ID returns 500")]
    public async Task UpdatePost_Put_NonExistentId_ReturnsInternalServerError()
    {
        var allPosts = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);
        var nonExistentId = allPosts.Data!.Max(post => post.Id) + 1;

        var response = await Put<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _putBody,
            IdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);
    }

    [Test]
    [Category("Negative")]
    [Description("5.11 PUT with non-numeric ID returns 500")]
    public async Task UpdatePost_Put_InvalidSegment_ReturnsInternalServerError()
    {
        var response = await Put<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _putBody,
            [UrlSegment("id", NonNumericId)]);

        response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);
    }

    [Test]
    [Category("Negative")]
    [Description("5.12 PUT with malformed JSON returns 500")]
    public async Task UpdatePost_Put_MalformedJson_ReturnsInternalServerError()
    {
        var request = new RestRequest(JsonPlaceholderEndpoints.PostsById, Method.Put);
        request.AddUrlSegment("id", TestPostId);
        request.AddStringBody(MalformedJson, ContentType.Json);

        var response = await Client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: PUT accepts body missing required fields — no validation — Bug: documentation/Bugs/JsonPlaceholder/JP-005-put-post-accepts-missing-required-fields.md")]
    [Category("Negative")]
    [Description("5.13 PUT without title returns 400")]
    public async Task UpdatePost_Put_MissingTitle_ReturnsBadRequest()
    {
        var body = new Dictionary<string, object> { ["body"] = "Body", ["userId"] = TestUserId };
        var response = await Put<Dictionary<string, object>, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, body,
            IdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: PUT accepts body missing required fields — no validation — Bug: documentation/Bugs/JsonPlaceholder/JP-005-put-post-accepts-missing-required-fields.md")]
    [Category("Negative")]
    [Description("5.14 PUT without body returns 400")]
    public async Task UpdatePost_Put_MissingBody_ReturnsBadRequest()
    {
        var body = new Dictionary<string, object> { ["title"] = "Title", ["userId"] = TestUserId };
        var response = await Put<Dictionary<string, object>, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, body,
            IdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: PUT accepts body missing required fields — no validation — Bug: documentation/Bugs/JsonPlaceholder/JP-005-put-post-accepts-missing-required-fields.md")]
    [Category("Negative")]
    [Description("5.15 PUT without userId returns 400")]
    public async Task UpdatePost_Put_MissingUserId_ReturnsBadRequest()
    {
        var body = new Dictionary<string, object> { ["title"] = "Title", ["body"] = "Body" };
        var response = await Put<Dictionary<string, object>, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, body,
            IdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Category("Negative")]
    [Description("5.16 PATCH with malformed JSON returns 500")]
    public async Task UpdatePost_Patch_MalformedJson_ReturnsInternalServerError()
    {
        var request = new RestRequest(JsonPlaceholderEndpoints.PostsById, Method.Patch);
        request.AddUrlSegment("id", TestPostId);
        request.AddStringBody(MalformedJson, ContentType.Json);

        var response = await Client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: PATCH returns 200 for any ID, including invalid and non-existent — Bug: documentation/Bugs/JsonPlaceholder/JP-006-patch-post-returns-200-for-any-id.md")]
    [Category("Negative")]
    [Description("5.17 PATCH with ID 0 returns 404")]
    public async Task UpdatePost_Patch_ZeroId_ReturnsNotFound()
    {
        var response = await Patch<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            IdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: PATCH returns 200 for any ID, including invalid and non-existent — Bug: documentation/Bugs/JsonPlaceholder/JP-006-patch-post-returns-200-for-any-id.md")]
    [Category("Negative")]
    [Description("5.18 PATCH with non-existent ID returns 404")]
    public async Task UpdatePost_Patch_NonExistentId_ReturnsNotFound()
    {
        var allPosts = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);
        var nonExistentId = allPosts.Data!.Max(post => post.Id) + 1;

        var response = await Patch<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            IdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("JsonPlaceholder mock: PATCH returns 200 for any ID, including invalid and non-existent — Bug: documentation/Bugs/JsonPlaceholder/JP-006-patch-post-returns-200-for-any-id.md")]
    [Category("Negative")]
    [Description("5.19 PATCH with non-numeric ID returns 404")]
    public async Task UpdatePost_Patch_InvalidSegment_ReturnsNotFound()
    {
        var response = await Patch<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.PostsById, _patchBody,
            [UrlSegment("id", NonNumericId)]);

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
