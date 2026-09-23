using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Photos;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetPhotoByIdTests : JsonPlaceholderRequestHelper
{
    private const int TestPhotoId = 1;
    private const int BoundaryPhotoId = 2500;
    private const int LastPhotoId = 5000;

    [Test]
    [Category("HealthCheck")]
    [Description("2.1 Status code is 200 for valid ID")]
    public async Task GetPhotoById_ReturnsOk()
    {
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(TestPhotoId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("2.2 Response matches expected contract")]
    public async Task GetPhotoById_ResponseMatchesContract()
    {
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(TestPhotoId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("2.3 Response body is not null")]
    public async Task GetPhotoById_ReturnsNonNull()
    {
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(TestPhotoId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("2.4 Each field has valid attributes")]
    public async Task GetPhotoById_HasValidFields()
    {
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(TestPhotoId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.5 Response contains correct ID")]
    public async Task GetPhotoById_ReturnsCorrectId()
    {
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(TestPhotoId));

        response.Data!.Id.Should().Be(TestPhotoId);
    }

    [Test]
    [Category("Negative")]
    [Description("2.6 Returns 404 for non-existent ID")]
    public async Task GetPhotoById_NonExistentId_ReturnsNotFound()
    {
        var allPhotos = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);
        var maxPhotoId = allPhotos.Data!.Max(p => p.Id);
        var nonExistentId = maxPhotoId + 1;

        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.7 Returns 404 for ID 0")]
    public async Task GetPhotoById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Negative")]
    [Description("2.8 Returns 404 for negative ID")]
    public async Task GetPhotoById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("2.9 Get photo by ID = 2500 returns valid photo")]
    public async Task GetPhotoById_BoundaryId_ReturnsValidPhoto()
    {
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(BoundaryPhotoId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(BoundaryPhotoId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.10 Get photo by ID = 5000 (last) returns valid photo")]
    public async Task GetPhotoById_LastId_ReturnsValidPhoto()
    {
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(LastPhotoId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(LastPhotoId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.11 Repeated calls return same data")]
    public async Task GetPhotoById_RepeatedCalls_ReturnSameData()
    {
        var response1 = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(TestPhotoId));
        var response2 = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(TestPhotoId));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Title.Should().Be(response2.Data!.Title);
        response1.Data!.Url.Should().Be(response2.Data!.Url);
    }

    [Test]
    [Category("Performance")]
    [Description("2.12 Response time < 5 seconds")]
    public async Task GetPhotoById_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<PhotoModelResponse>(JsonPlaceholderEndpoints.PhotosById,
            PostIdParam(TestPhotoId));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
