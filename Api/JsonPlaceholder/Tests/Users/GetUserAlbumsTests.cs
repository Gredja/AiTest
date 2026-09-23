using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Users;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetUserAlbumsTests : JsonPlaceholderRequestHelper
{
    private const int TestUserId = 1;

    [Test]
    [Category("HealthCheck")]
    [Description("5.1 Status code is 200")]
    public async Task GetUserAlbums_ReturnsOk()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.UsersAlbums,
            PostIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("5.2 Response matches expected contract")]
    public async Task GetUserAlbums_ResponseMatchesContract()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.UsersAlbums,
            PostIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("5.3 Response body is not empty")]
    public async Task GetUserAlbums_ReturnsNonEmptyList()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.UsersAlbums,
            PostIdParam(TestUserId));

        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("5.4 Each item has valid fields")]
    public async Task GetUserAlbums_EachItemHasValidFields()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.UsersAlbums,
            PostIdParam(TestUserId));

        foreach (var album in response.Data!)
        {
            album.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("5.5 All albums belong to same user")]
    public async Task GetUserAlbums_AllBelongToSameUser()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.UsersAlbums,
            PostIdParam(TestUserId));

        response.Data!.Should().OnlyContain(a => a.UserId == TestUserId);
    }

    [Test]
    [Category("Negative")]
    [Description("5.6 Returns empty list for non-existent userId")]
    public async Task GetUserAlbums_NonExistentUserId_ReturnsEmpty()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.UsersAlbums,
            PostIdParam(999999));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("5.7 Returns empty list for userId=0")]
    public async Task GetUserAlbums_ZeroUserId_ReturnsEmpty()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.UsersAlbums,
            PostIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }
}
