using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Comments;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAllCommentsTests : JsonPlaceholderRequestHelper
{
    private const int ExpectedCommentCount = 500;

    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAllComments_ReturnsOk()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("1.2 Response matches expected contract")]
    public async Task GetAllComments_ResponseMatchesContract()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Response body is not empty")]
    public async Task GetAllComments_ReturnsNonEmptyList()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.4 Content-Type is application/json")]
    public async Task GetAllComments_ContentTypeIsJson()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("1.5 Each item has valid required fields (via attributes)")]
    public async Task GetAllComments_EachItemHasValidFields()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);

        foreach (var comment in response.Data!)
        {
            comment.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("1.6 Returns exactly 500 comments")]
    public async Task GetAllComments_ReturnsExpectedCount()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(ExpectedCommentCount);
    }

    [Test]
    [Category("Regression")]
    [Description("1.7 All comment IDs are unique")]
    public async Task GetAllComments_AllIdsAreUnique()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);

        var ids = response.Data!.Select(c => c.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("1.8 Each comment has postId > 0")]
    public async Task GetAllComments_EachCommentHasValidPostId()
    {
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);

        response.Data.Should().OnlyContain(c => c.PostId > 0);
    }

    [Test]
    [Category("Performance")]
    [Description("1.9 Response time < 5 seconds")]
    public async Task GetAllComments_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<Comment>>(JsonPlaceholderEndpoints.Comments);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
