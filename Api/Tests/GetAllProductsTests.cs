using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Config;
using Api.Helpers;
using Core.Helpers;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;

namespace Api.Tests;

[TestFixture]
public class GetAllProductsTests
{
    private RestClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _client = new RestClient(Endpoints.BaseUrl);
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    [Description("1.1 Status code is 200")]
    public async Task GetAllProducts_ReturnsOk()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.ShouldHaveStatusCode(System.Net.HttpStatusCode.OK);
    }

    [Test]
    [Description("1.2 Response body is not empty")]
    public async Task GetAllProducts_ReturnsNonEmptyList()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.ShouldHaveStatusCode(System.Net.HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Description("1.3 Response is a JSON array")]
    public async Task GetAllProducts_ResponseIsJsonArray()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.Data.Should().BeOfType<List<ProductModel>>();
    }

    [Test]
    [Description("1.4 Each item has `id` (integer)")]
    public async Task GetAllProducts_EachItemHasId()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.Data!.ShouldAllHaveValidProducts();
    }

    [Test]
    [Description("1.5 Each item has `title` (string, not empty)")]
    public async Task GetAllProducts_EachItemHasTitle()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.Data!.ShouldAllHaveValidProducts();
    }

    [Test]
    [Description("1.6 Each item has `price` (number, >= 0)")]
    public async Task GetAllProducts_EachItemHasPrice()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.Data!.ShouldAllHaveValidProducts();
    }

    [Test]
    [Description("1.7 Each item has `description` (string)")]
    public async Task GetAllProducts_EachItemHasDescription()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.Data!.ShouldAllHaveValidProducts();
    }

    [Test]
    [Description("1.8 Each item has `category` (string, not empty)")]
    public async Task GetAllProducts_EachItemHasCategory()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.Data!.ShouldAllHaveValidProducts();
    }

    [Test]
    [Description("1.9 Each item has `image` (string, valid URL)")]
    public async Task GetAllProducts_EachItemHasImage()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.Data!.ShouldAllHaveValidProducts();
    }

    [Test]
    [Description("1.10 Each item has `rating` (object with `rate` 0-5, `count` >= 0)")]
    public async Task GetAllProducts_EachItemHasRating()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = await _client.ExecuteAsync<List<ProductModel>>(request);

        response.Data!.ShouldAllHaveValidProducts();
    }

    [Test]
    [Description("1.11 Response time < 5 seconds")]
    public async Task GetAllProducts_ResponseTimeIsAcceptable()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var stopwatch = Stopwatch.StartNew();

        var response = await _client.ExecuteAsync<List<ProductModel>>(request);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }
}
