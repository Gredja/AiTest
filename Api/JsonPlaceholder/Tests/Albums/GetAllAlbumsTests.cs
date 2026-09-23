using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Albums;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAllAlbumsTests : JsonPlaceholderRequestHelper
{
    private const int ExpectedAlbumCount = 100;

    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAllAlbums_ReturnsOk()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("1.2 Response matches expected contract")]
    public async Task GetAllAlbums_ResponseMatchesContract()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Response body is not empty")]
    public async Task GetAllAlbums_ReturnsNonEmptyList()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.4 Content-Type is application/json")]
    public async Task GetAllAlbums_ContentTypeIsJson()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("1.5 Each item has valid required fields (via attributes)")]
    public async Task GetAllAlbums_EachItemHasValidFields()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);

        foreach (var album in response.Data!)
        {
            album.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("1.6 Returns exactly 100 albums")]
    public async Task GetAllAlbums_ReturnsExpectedCount()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(ExpectedAlbumCount);
    }

    [Test]
    [Category("Regression")]
    [Description("1.7 All album IDs are unique")]
    public async Task GetAllAlbums_AllIdsAreUnique()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);

        var ids = response.Data!.Select(a => a.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("1.8 Each album has userId > 0")]
    public async Task GetAllAlbums_EachAlbumHasValidUserId()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);

        response.Data.Should().OnlyContain(a => a.UserId > 0);
    }

    [Test]
    [Category("Performance")]
    [Description("1.9 Response time < 5 seconds")]
    public async Task GetAllAlbums_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
