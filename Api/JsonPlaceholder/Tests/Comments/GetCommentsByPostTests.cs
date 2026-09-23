using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Comments;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetCommentsByPostTests : JsonPlaceholderRequestHelper
{
    private const int TestPostId = 1;

    [Test]
    [Category("HealthCheck")]
    [Description("3.1 Status code is 200")]
    public async Task GetCommentsByPost_ReturnsOk()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.CommentsByPost,
            PostIdQueryParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("3.2 Response matches expected contract")]
    public async Task GetCommentsByPost_ResponseMatchesContract()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.CommentsByPost,
            PostIdQueryParam(TestPostId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("3.3 Response body is not empty")]
    public async Task GetCommentsByPost_ReturnsNonEmptyList()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.CommentsByPost,
            PostIdQueryParam(TestPostId));

        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("3.4 Each item has valid fields")]
    public async Task GetCommentsByPost_EachItemHasValidFields()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.CommentsByPost,
            PostIdQueryParam(TestPostId));

        foreach (var comment in response.Data!)
        {
            comment.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("3.5 All comments belong to same post")]
    public async Task GetCommentsByPost_AllBelongToSamePost()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.CommentsByPost,
            PostIdQueryParam(TestPostId));

        response.Data!.Should().OnlyContain(c => c.PostId == TestPostId);
    }

    [Test]
    [Category("Negative")]
    [Description("3.6 Returns empty list for non-existent postId")]
    public async Task GetCommentsByPost_NonExistentPostId_ReturnsEmpty()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.CommentsByPost,
            UserIdParam(999999));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("3.7 Returns empty list for postId=0")]
    public async Task GetCommentsByPost_ZeroPostId_ReturnsEmpty()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.CommentsByPost,
            UserIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }
}
