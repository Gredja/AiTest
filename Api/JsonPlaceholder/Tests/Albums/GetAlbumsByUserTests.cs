using RestSharp;
using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;

namespace Api.JsonPlaceholder.Albums;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAlbumsByUserTests : JsonPlaceholderRequestHelper
{
    private const int TestUserId = 1;

    [Test]
    [Category("HealthCheck")]
    [Description("3.1 Status code is 200")]
    public async Task GetAlbumsByUser_ReturnsOk()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.AlbumsByUser,
            UserIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("3.2 Response matches expected contract")]
    public async Task GetAlbumsByUser_ResponseMatchesContract()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.AlbumsByUser,
            UserIdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("3.3 Response body is not empty")]
    public async Task GetAlbumsByUser_ReturnsNonEmptyList()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.AlbumsByUser,
            UserIdParam(TestUserId));

        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("3.4 Each item has valid fields")]
    public async Task GetAlbumsByUser_EachItemHasValidFields()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.AlbumsByUser,
            UserIdParam(TestUserId));

        foreach (var album in response.Data!)
        {
            album.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("3.5 All albums belong to same user")]
    public async Task GetAlbumsByUser_AllBelongToSameUser()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.AlbumsByUser,
            UserIdParam(TestUserId));

        response.Data!.Should().OnlyContain(a => a.UserId == TestUserId);
    }

    [Test]
    [Category("Negative")]
    [Description("3.6 Returns empty list for non-existent userId")]
    public async Task GetAlbumsByUser_NonExistentUserId_ReturnsEmpty()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.AlbumsByUser,
            UserIdParam(999999));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("3.7 Returns empty list for userId=0")]
    public async Task GetAlbumsByUser_ZeroUserId_ReturnsEmpty()
    {
        var response = await Get<List<AlbumModelResponse>>(JsonPlaceholderEndpoints.AlbumsByUser,
            UserIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }
}
