using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.FakeStore.Tests;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetProductByIdTests : RequestHelper
{
    private static List<RequestDictionaryModel> ProductIdParam(int id) =>
        new() { new() { Type = "UrlSegment", Key = "id", Value = id } };

    [Test]
    [Category("HealthCheck")]
    [Description("2.1 Get product by ID = 1 — status code 200")]
    public async Task GetProductById_ValidId_ReturnsOk()
    {
        var response = await Get<ProductModel>(FakeStoreEndpoints.ProductsById, Method.Get, ProductIdParam(FakeStoreEndpoints.TestProductId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("2.2 Response body is not empty")]
    public async Task GetProductById_ValidId_ReturnsNonEmptyBody()
    {
        var response = await Get<ProductModel>(FakeStoreEndpoints.ProductsById, Method.Get, ProductIdParam(FakeStoreEndpoints.TestProductId));

        response.ShouldBeOk();
    }

    [Test]
    [Category("Regression")]
    [Description("2.3 Response has all expected fields")]
    public async Task GetProductById_ValidId_HasAllExpectedFields()
    {
        var response = await Get<ProductModel>(FakeStoreEndpoints.ProductsById, Method.Get, ProductIdParam(FakeStoreEndpoints.TestProductId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.4 `id` in response matches requested ID")]
    public async Task GetProductById_ValidId_IdMatchesRequested()
    {
        var response = await Get<ProductModel>(FakeStoreEndpoints.ProductsById, Method.Get, ProductIdParam(FakeStoreEndpoints.TestProductId));

        response.Data!.Id.Should().Be(FakeStoreEndpoints.TestProductId);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
    [Category("Negative")]
    [Description("2.5 Get product by non-existent ID (maxId + 1) — status code 404")]
    public async Task GetProductById_NonExistentId_ReturnsNotFound()
    {
        var allProducts = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);
        var maxId = allProducts.Data!.Max(p => p.Id);
        var nonExistentId = maxId + 1;

        var response = await Get<ProductModel>(FakeStoreEndpoints.ProductsById, Method.Get, ProductIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for ID=0")]
    [Category("Negative")]
    [Description("2.6 Get product by ID = 0 — status code 404")]
    public async Task GetProductById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<ProductModel>(FakeStoreEndpoints.ProductsById, Method.Get, ProductIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for negative IDs")]
    [Category("Negative")]
    [Description("2.7 Get product by negative ID (-1) — status code 404")]
    public async Task GetProductById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<ProductModel>(FakeStoreEndpoints.ProductsById, Method.Get, ProductIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
