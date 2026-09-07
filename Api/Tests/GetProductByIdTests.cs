using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.Tests;

[TestFixture]
[AllureNUnit]
public class GetProductByIdTests : RequestHelper
{
    private static List<RequestDictionaryModel> ProductIdParam(int id) =>
        new() { new() { Type = "UrlSegment", Key = "id", Value = id } };

    [Test]
    [Description("2.1 Get product by ID = 1 — status code 200")]
    public async Task GetProductById_ValidId_ReturnsOk()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(Endpoints.TestProductId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Description("2.2 Response body is not empty")]
    public async Task GetProductById_ValidId_ReturnsNonEmptyBody()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(Endpoints.TestProductId));

        response.ShouldBeOk();
    }

    [Test]
    [Description("2.3 Response has all expected fields")]
    public async Task GetProductById_ValidId_HasAllExpectedFields()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(Endpoints.TestProductId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Description("2.4 `id` in response matches requested ID")]
    public async Task GetProductById_ValidId_IdMatchesRequested()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(Endpoints.TestProductId));

        response.Data!.Id.Should().Be(Endpoints.TestProductId);
    }

    [Test]
    [Description("2.5 `title` is string, not empty")]
    public async Task GetProductById_ValidId_HasTitle()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(Endpoints.TestProductId));

        response.Data!.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Description("2.6 `price` is number, >= 0")]
    public async Task GetProductById_ValidId_HasPrice()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(Endpoints.TestProductId));

        response.Data!.Price.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    [Description("2.7 `category` is string, not empty")]
    public async Task GetProductById_ValidId_HasCategory()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(Endpoints.TestProductId));

        response.Data!.Category.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Description("2.8 `rating.rate` is number, 0-5")]
    public async Task GetProductById_ValidId_HasRating()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(Endpoints.TestProductId));

        response.Data!.Rating.Should().NotBeNull();
        response.Data.Rating.Rate.Should().BeInRange(0, 5);
        response.Data.Rating.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
    [Description("2.9 Get product by non-existent ID (maxId + 1) — status code 404")]
    public async Task GetProductById_NonExistentId_ReturnsNotFound()
    {
        var allProducts = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);
        var maxId = allProducts.Data!.Max(p => p.Id);
        var nonExistentId = maxId + 1;

        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for ID=0")]
    [Description("2.10 Get product by ID = 0 — status code 404")]
    public async Task GetProductById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for negative IDs")]
    [Description("2.11 Get product by negative ID (-1) — status code 404")]
    public async Task GetProductById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<ProductModel>(Endpoints.ProductsById, Method.Get, ProductIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
