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
public class DeletePostTests : JsonPlaceholderRequestHelper
{
    [Test]
    [Description("6.1 DELETE returns 200 OK")]
    public async Task DeletePost_ReturnsOk()
    {
        var response = await Delete<PostModel>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Description("6.2 DELETE response is empty object")]
    public async Task DeletePost_ReturnsEmptyObject()
    {
        var response = await Delete<PostModel>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.Data.Should().NotBeNull();
    }

    private static List<RequestDictionaryModel> PostIdParam(int id)
    {
        return new List<RequestDictionaryModel>
        {
            new() { Type = "UrlSegment", Key = "id", Value = id }
        };
    }
}
