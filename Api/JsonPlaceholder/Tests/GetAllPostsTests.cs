using NUnit.Framework;
using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Tests;

[TestFixture]
[AllureNUnit]
public class GetAllPostsTests : JsonPlaceholderRequestHelper
{
    [Test]
    [Description("1.1 Status code is 200")]
    public async Task GetAllPosts_ReturnsOk()
    {
        var response = await Get<List<PostModel>>(JsonPlaceholderEndpoints.Posts, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Description("1.2 Response body is not empty")]
    public async Task GetAllPosts_ReturnsNonEmptyList()
    {
        var response = await Get<List<PostModel>>(JsonPlaceholderEndpoints.Posts, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Description("1.3 Content-Type is application/json")]
    public async Task GetAllPosts_ContentTypeIsJson()
    {
        var response = await Get<List<PostModel>>(JsonPlaceholderEndpoints.Posts, Method.Get);

        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Description("1.4 Each item has valid required fields (via attributes)")]
    public async Task GetAllPosts_EachItemHasValidFields()
    {
        var response = await Get<List<PostModel>>(JsonPlaceholderEndpoints.Posts, Method.Get);

        foreach (var post in response.Data!)
        {
            post.ShouldHaveValidFields();
        }
    }

    [Test]
    [Description("1.5 Response time < 5 seconds")]
    public async Task GetAllPosts_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<PostModel>>(JsonPlaceholderEndpoints.Posts, Method.Get);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(JsonPlaceholderEndpoints.MaxResponseTimeMs);
    }

    [Test]
    [Description("1.9 Returns exactly 100 posts")]
    public async Task GetAllPosts_ReturnsExpectedCount()
    {
        var response = await Get<List<PostModel>>(JsonPlaceholderEndpoints.Posts, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(JsonPlaceholderEndpoints.ExpectedPostCount);
    }
}
