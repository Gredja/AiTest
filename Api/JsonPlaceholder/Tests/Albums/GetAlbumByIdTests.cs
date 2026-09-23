using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Albums;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAlbumByIdTests : JsonPlaceholderRequestHelper
{
    private const int TestAlbumId = 1;
    private const int BoundaryAlbumId = 50;
    private const int LastAlbumId = 100;

    [Test]
    [Category("HealthCheck")]
    [Description("2.1 Status code is 200 for valid ID")]
    public async Task GetAlbumById_ReturnsOk()
    {
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(TestAlbumId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("2.2 Response matches expected contract")]
    public async Task GetAlbumById_ResponseMatchesContract()
    {
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(TestAlbumId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("2.3 Response body is not null")]
    public async Task GetAlbumById_ReturnsNonNull()
    {
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(TestAlbumId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("2.4 Each field has valid attributes")]
    public async Task GetAlbumById_HasValidFields()
    {
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(TestAlbumId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.5 Response contains correct ID")]
    public async Task GetAlbumById_ReturnsCorrectId()
    {
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(TestAlbumId));

        response.Data!.Id.Should().Be(TestAlbumId);
    }

    [Test]
    [Category("Negative")]
    [Description("2.6 Returns 404 for non-existent ID")]
    public async Task GetAlbumById_NonExistentId_ReturnsNotFound()
    {
        var allAlbums = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.Albums);
        var maxAlbumId = allAlbums.Data!.Max(a => a.Id);
        var nonExistentId = maxAlbumId + 1;

        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.7 Returns 404 for ID 0")]
    public async Task GetAlbumById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.8 Returns 404 for negative ID")]
    public async Task GetAlbumById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("2.9 Get album by ID = 50 returns valid album")]
    public async Task GetAlbumById_BoundaryId_ReturnsValidAlbum()
    {
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(BoundaryAlbumId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(BoundaryAlbumId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.10 Get album by ID = 100 (last) returns valid album")]
    public async Task GetAlbumById_LastId_ReturnsValidAlbum()
    {
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(LastAlbumId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(LastAlbumId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.11 Repeated calls return same data")]
    public async Task GetAlbumById_RepeatedCalls_ReturnSameData()
    {
        var response1 = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(TestAlbumId));
        var response2 = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(TestAlbumId));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Title.Should().Be(response2.Data!.Title);
    }

    [Test]
    [Category("Performance")]
    [Description("2.12 Response time < 5 seconds")]
    public async Task GetAlbumById_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<AlbumModelResponse>(JsonPlaceholderEndpoints.AlbumsById,
            PostIdParam(TestAlbumId));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
