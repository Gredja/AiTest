using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Tests;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class DeletePostTests : JsonPlaceholderRequestHelper
{
    [Test]
    [Category("HealthCheck")]
    [Description("6.1 DELETE returns 200 OK")]
    public async Task DeletePost_ReturnsOk()
    {
        var response = await Delete<PostModel>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("6.2 DELETE response is empty object")]
    public async Task DeletePost_ReturnsEmptyObject()
    {
        var response = await Delete<PostModel>(JsonPlaceholderEndpoints.PostsById,
            PostIdParam(JsonPlaceholderEndpoints.TestPostId));

        response.Data.Should().NotBeNull();
    }
}
