using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Config;
using System.Net;
using System.Diagnostics;
using FluentAssertions;

namespace Api.Tests;

[TestFixture]
public class ProductApiTests
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

    // GET /products

    [Test]
    [Description("1.1 Status code is 200")]
    public void GetAllProducts_ReturnsOk()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    [Description("1.2 Response body is not empty")]
    public void GetAllProducts_ReturnsNonEmptyList()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.Data.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Description("1.3 Response is a JSON array")]
    public void GetAllProducts_ResponseIsJsonArray()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.Data.Should().BeOfType<List<ProductModel>>();
    }

    [Test]
    [Description("1.4 Each item has `id` (integer)")]
    public void GetAllProducts_EachItemHasId()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.Data.Should().OnlyContain(p => p.Id > 0);
    }

    [Test]
    [Description("1.5 Each item has `title` (string, not empty)")]
    public void GetAllProducts_EachItemHasTitle()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.Data.Should().OnlyContain(p =>
            !string.IsNullOrWhiteSpace(p.Title));
    }

    [Test]
    [Description("1.6 Each item has `price` (number, >= 0)")]
    public void GetAllProducts_EachItemHasPrice()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.Data.Should().OnlyContain(p => p.Price >= 0);
    }

    [Test]
    [Description("1.7 Each item has `description` (string)")]
    public void GetAllProducts_EachItemHasDescription()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.Data.Should().OnlyContain(p =>
            !string.IsNullOrWhiteSpace(p.Description));
    }

    [Test]
    [Description("1.8 Each item has `category` (string, not empty)")]
    public void GetAllProducts_EachItemHasCategory()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.Data.Should().OnlyContain(p =>
            !string.IsNullOrWhiteSpace(p.Category));
    }

    [Test]
    [Description("1.9 Each item has `image` (string, valid URL)")]
    public void GetAllProducts_EachItemHasImage()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.Data.Should().OnlyContain(p =>
            !string.IsNullOrWhiteSpace(p.Image) &&
            p.Image.StartsWith("http"));
    }

    [Test]
    [Description("1.10 Each item has `rating` (object with `rate` 0-5, `count` >= 0)")]
    public void GetAllProducts_EachItemHasRating()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var response = _client.Execute<List<ProductModel>>(request);

        response.Data.Should().OnlyContain(p =>
            p.Rating != null &&
            p.Rating.Rate >= 0 && p.Rating.Rate <= 5 &&
            p.Rating.Count >= 0);
    }

    [Test]
    [Description("1.11 Response time < 5 seconds")]
    public void GetAllProducts_ResponseTimeIsAcceptable()
    {
        var request = new RestRequest(Endpoints.Products, Method.Get);
        var stopwatch = Stopwatch.StartNew();

        var response = _client.Execute<List<ProductModel>>(request);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    // GET /products/{id}

    [Test]
    [Description("2.1 Get product by ID = 1 — status code 200")]
    public void GetProductById_ValidId_ReturnsOk()
    {
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", 1);

        var response = _client.Execute<ProductModel>(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    [Description("2.2 Response body is not empty")]
    public void GetProductById_ValidId_ReturnsNonEmptyBody()
    {
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", 1);

        var response = _client.Execute<ProductModel>(request);

        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().BeGreaterThan(0);
    }

    [Test]
    [Description("2.3 Response has all expected fields")]
    public void GetProductById_ValidId_HasAllExpectedFields()
    {
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", 1);

        var response = _client.Execute<ProductModel>(request);

        var product = response.Data!;
        product.Id.Should().BeGreaterThan(0);
        product.Title.Should().NotBeNullOrWhiteSpace();
        product.Price.Should().BeGreaterThanOrEqualTo(0);
        product.Description.Should().NotBeNull();
        product.Category.Should().NotBeNullOrWhiteSpace();
        product.Image.Should().NotBeNullOrWhiteSpace();
        product.Rating.Should().NotBeNull();
    }

    [Test]
    [Description("2.4 `id` in response matches requested ID")]
    public void GetProductById_ValidId_IdMatchesRequested()
    {
        var requestedId = 1;
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", requestedId);

        var response = _client.Execute<ProductModel>(request);

        response.Data!.Id.Should().Be(requestedId);
    }

    [Test]
    [Description("2.5 `title` is string, not empty")]
    public void GetProductById_ValidId_HasTitle()
    {
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", 1);

        var response = _client.Execute<ProductModel>(request);

        response.Data!.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Description("2.6 `price` is number, >= 0")]
    public void GetProductById_ValidId_HasPrice()
    {
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", 1);

        var response = _client.Execute<ProductModel>(request);

        response.Data!.Price.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    [Description("2.7 `category` is string, not empty")]
    public void GetProductById_ValidId_HasCategory()
    {
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", 1);

        var response = _client.Execute<ProductModel>(request);

        response.Data!.Category.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Description("2.8 `rating.rate` is number, 0-5")]
    public void GetProductById_ValidId_HasRating()
    {
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", 1);

        var response = _client.Execute<ProductModel>(request);

        response.Data!.Rating.Should().NotBeNull();
        response.Data.Rating.Rate.Should().BeInRange(0, 5);
        response.Data.Rating.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
    [Description("2.9 Get product by non-existent ID (maxId + 1) — status code 404")]
    public void GetProductById_NonExistentId_ReturnsNotFound()
    {
        var getAllRequest = new RestRequest(Endpoints.Products, Method.Get);
        var allProducts = _client.Execute<List<ProductModel>>(getAllRequest);
        var maxId = allProducts.Data!.Max(p => p.Id);
        var nonExistentId = maxId + 1;

        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", nonExistentId);

        var response = _client.Execute<ProductModel>(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for ID=0")]
    [Description("2.10 Get product by ID = 0 — status code 404")]
    public void GetProductById_ZeroId_ReturnsNotFound()
    {
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", 0);

        var response = _client.Execute<ProductModel>(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for negative IDs")]
    [Description("2.11 Get product by negative ID (-1) — status code 404")]
    public void GetProductById_NegativeId_ReturnsNotFound()
    {
        var request = new RestRequest(Endpoints.ProductsById, Method.Get);
        request.AddUrlSegment("id", -1);

        var response = _client.Execute<ProductModel>(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
