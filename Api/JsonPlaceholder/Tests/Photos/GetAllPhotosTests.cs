using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.JsonPlaceholder.Photos;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAllPhotosTests : JsonPlaceholderRequestHelper
{
    private const int ExpectedPhotoCount = 5000;

    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAllPhotos_ReturnsOk()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("1.2 Response matches expected contract")]
    public async Task GetAllPhotos_ResponseMatchesContract()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Response body is not empty")]
    public async Task GetAllPhotos_ReturnsNonEmptyList()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.4 Content-Type is application/json")]
    public async Task GetAllPhotos_ContentTypeIsJson()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("1.5 Each item has valid required fields (via attributes)")]
    public async Task GetAllPhotos_EachItemHasValidFields()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);

        foreach (var photo in response.Data!)
        {
            photo.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("1.6 Returns exactly 5000 photos")]
    public async Task GetAllPhotos_ReturnsExpectedCount()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(ExpectedPhotoCount);
    }

    [Test]
    [Category("Regression")]
    [Description("1.7 All photo IDs are unique")]
    public async Task GetAllPhotos_AllIdsAreUnique()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);

        var ids = response.Data!.Select(p => p.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("1.8 Each photo has albumId > 0")]
    public async Task GetAllPhotos_EachPhotoHasValidAlbumId()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);

        response.Data.Should().OnlyContain(p => p.AlbumId > 0);
    }

    [Test]
    [Category("Performance")]
    [Description("1.9 Response time < 5 seconds")]
    public async Task GetAllPhotos_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.Photos);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
