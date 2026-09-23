using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Photos;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetPhotosByAlbumTests : JsonPlaceholderRequestHelper
{
    private const int TestAlbumId = 1;

    [Test]
    [Category("HealthCheck")]
    [Description("3.1 Status code is 200")]
    public async Task GetPhotosByAlbum_ReturnsOk()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.PhotosByAlbum,
            AlbumIdQueryParam(TestAlbumId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("3.2 Response matches expected contract")]
    public async Task GetPhotosByAlbum_ResponseMatchesContract()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.PhotosByAlbum,
            AlbumIdQueryParam(TestAlbumId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("3.3 Response body is not empty")]
    public async Task GetPhotosByAlbum_ReturnsNonEmptyList()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.PhotosByAlbum,
            AlbumIdQueryParam(TestAlbumId));

        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("3.4 Each item has valid fields")]
    public async Task GetPhotosByAlbum_EachItemHasValidFields()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.PhotosByAlbum,
            AlbumIdQueryParam(TestAlbumId));

        foreach (var photo in response.Data!)
        {
            photo.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("3.5 All photos belong to same album")]
    public async Task GetPhotosByAlbum_AllBelongToSameAlbum()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.PhotosByAlbum,
            AlbumIdQueryParam(TestAlbumId));

        response.Data!.Should().OnlyContain(p => p.AlbumId == TestAlbumId);
    }

    [Test]
    [Category("Negative")]
    [Description("3.6 Returns empty list for non-existent albumId")]
    public async Task GetPhotosByAlbum_NonExistentAlbumId_ReturnsEmpty()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.PhotosByAlbum,
            UserIdParam(999999));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("3.7 Returns empty list for albumId=0")]
    public async Task GetPhotosByAlbum_ZeroAlbumId_ReturnsEmpty()
    {
        var response = await Get<List<PhotoModelResponse>>(JsonPlaceholderEndpoints.PhotosByAlbum,
            UserIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }
}
