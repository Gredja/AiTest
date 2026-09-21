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
public class GetPostByIdTests : JsonPlaceholderRequestHelper
{
    private const int TestPostId = 1;
    private const int BoundaryPostId = 50;
    private const int LastPostId = 100;
    [Test]
    [Category("HealthCheck")]
    [Description("2.1 Status code is 200 for valid ID")]
    public async Task GetPostById_ReturnsOk()
    {
        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("2.2 Response matches expected contract")]
    public async Task GetPostById_ResponseMatchesContract()
    {
        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("2.3 Response body is not null")]
    public async Task GetPostById_ReturnsNonNull()
    {
        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("2.4 Each field has valid attributes")]
    public async Task GetPostById_HasValidFields()
    {
        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.5 Response contains correct ID")]
    public async Task GetPostById_ReturnsCorrectId()
    {
        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        response.Data!.Id.Should().Be(TestPostId);
    }

    [Test]
    [Category("Negative")]
    [Description("2.6 Returns 404 for non-existent ID")]
    public async Task GetPostById_NonExistentId_ReturnsNotFound()
    {
        var allPosts = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);
        var maxPostId = allPosts.Data!.Max(p => p.Id);
        var nonExistentId = maxPostId + 1;

        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.7 Returns 404 for ID 0")]
    public async Task GetPostById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.8 Returns 404 for negative ID")]
    public async Task GetPostById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("2.9 Get post by ID = 50 returns valid post")]
    public async Task GetPostById_BoundaryId_ReturnsValidPost()
    {
        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(BoundaryPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(BoundaryPostId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.10 Get post by ID = 100 (last) returns valid post")]
    public async Task GetPostById_LastId_ReturnsValidPost()
    {
        var response = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(LastPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(LastPostId);
    }

    [Test]
    [Category("Regression")]
    [Description("2.11 Repeated calls return same data")]
    public async Task GetPostById_RepeatedCalls_ReturnSameData()
    {
        var response1 = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));
        var response2 = await Get<PostModelResponse>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(TestPostId));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Title.Should().Be(response2.Data!.Title);
        response1.Data!.Body.Should().Be(response2.Data!.Body);
    }
}
