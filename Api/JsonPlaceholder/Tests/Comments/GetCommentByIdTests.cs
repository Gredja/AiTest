using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Comments;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetCommentByIdTests : JsonPlaceholderRequestHelper
{
    private const int TestCommentId = 1;
    private const int BoundaryCommentId = 250;
    private const int LastCommentId = 500;

    [Test]
    [Category("HealthCheck")]
    [Description("2.1 Status code is 200 for valid ID")]
    public async Task GetCommentById_ReturnsOk()
    {
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(TestCommentId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("2.2 Response matches expected contract")]
    public async Task GetCommentById_ResponseMatchesContract()
    {
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(TestCommentId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("2.3 Response body is not null")]
    public async Task GetCommentById_ReturnsNonNull()
    {
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(TestCommentId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("2.4 Each field has valid attributes")]
    public async Task GetCommentById_HasValidFields()
    {
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(TestCommentId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.5 Response contains correct ID")]
    public async Task GetCommentById_ReturnsCorrectId()
    {
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(TestCommentId));

        response.Data!.Id.Should().Be(TestCommentId);
    }

    [Test]
    [Category("Negative")]
    [Description("2.6 Returns 404 for non-existent ID")]
    public async Task GetCommentById_NonExistentId_ReturnsNotFound()
    {
        var allComments = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);
        var maxCommentId = allComments.Data!.Max(c => c.Id);
        var nonExistentId = maxCommentId + 1;

        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.7 Returns 404 for ID 0")]
    public async Task GetCommentById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.8 Returns 404 for negative ID")]
    public async Task GetCommentById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("2.9 Get comment by ID = 250 returns valid comment")]
    public async Task GetCommentById_BoundaryId_ReturnsValidComment()
    {
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(BoundaryCommentId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(BoundaryCommentId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.10 Get comment by ID = 500 (last) returns valid comment")]
    public async Task GetCommentById_LastId_ReturnsValidComment()
    {
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(LastCommentId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(LastCommentId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.11 Repeated calls return same data")]
    public async Task GetCommentById_RepeatedCalls_ReturnSameData()
    {
        var response1 = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(TestCommentId));
        var response2 = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(TestCommentId));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Name.Should().Be(response2.Data!.Name);
        response1.Data!.Email.Should().Be(response2.Data!.Email);
    }

    [Test]
    [Category("Performance")]
    [Description("2.12 Response time < 5 seconds")]
    public async Task GetCommentById_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<Comment>(JsonPlaceholderEndpoints.CommentsById,
            PostIdParam(TestCommentId));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
