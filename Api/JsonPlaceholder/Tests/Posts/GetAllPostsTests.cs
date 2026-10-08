using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderTestData;
using static Core.Helpers.ParamHelper;

namespace Api.JsonPlaceholder.Posts;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAllPostsTests : JsonPlaceholderRequestHelper
{
    private const int ExpectedPostCount = 100;
    private int? _createdPostId;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);

        if (response.Data!.Count == 0)
        {
            var create = await Post<PostModelRequest, PostModelResponse>(JsonPlaceholderEndpoints.Posts, TestPost);
            create.ShouldHaveStatusCode(HttpStatusCode.Created);
            _createdPostId = create.Data!.Id;
        }
    }

    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAllPosts_ReturnsOk()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("1.2 Response matches expected contract")]
    public async Task GetAllPosts_ResponseMatchesContract()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Response body is not empty")]
    public async Task GetAllPosts_ReturnsNonEmptyList()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.4 Content-Type is application/json")]
    public async Task GetAllPosts_ContentTypeIsJson()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("1.5 Each item has valid required fields (via attributes)")]
    public async Task GetAllPosts_EachItemHasValidFields()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);

        foreach (var post in response.Data!)
        {
            post.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Performance")]
    [Description("1.6 Response time < 5 seconds")]
    public async Task GetAllPosts_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.7 Returns exactly 100 posts")]
    public async Task GetAllPosts_ReturnsExpectedCount()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(ExpectedPostCount);
    }

    [Test]
    [Category("Regression")]
    [Description("1.8 All post IDs are unique")]
    public async Task GetAllPosts_AllIdsAreUnique()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);

        var ids = response.Data!.Select(post => post.Id);
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("1.9 Each post has userId > 0")]
    public async Task GetAllPosts_EachPostHasValidUserId()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.Posts);

        response.Data.Should().OnlyContain(post => post.UserId > 0);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdPostId.HasValue)
        {
            try
            {
                var delete = await Delete<object>(JsonPlaceholderEndpoints.PostsById, IdParam(_createdPostId.Value));

                if (!delete.IsSuccessful)
                {
                    TestContext.Progress.WriteLine(
                        $"Warning: post {_createdPostId} not deleted: HTTP {(int)delete.StatusCode}");
                }
            }
            catch (HttpRequestException exception)
            {
                TestContext.Progress.WriteLine(
                    $"Warning: failed to delete post {_createdPostId}: {exception.Message}");
            }
        }
    }
}
