using RestSharp;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.FakeStore.Helpers.FakeStoreParamHelper;

namespace Api.FakeStore.Products;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetProductByIdTests : RequestHelper
{
    private const int TestProductId = 1;
    private const int BoundaryProductId = 5;
    private const int LastProductId = 20;
    [Test]
    [Category("HealthCheck")]
    [Description("2.1 Get product by ID = 1 — status code 200")]
    public async Task GetProductById_ValidId_ReturnsOk()
    {
        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(TestProductId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("2.2 Response matches expected contract")]
    public async Task GetProductById_ResponseMatchesContract()
    {
        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(TestProductId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("2.3 Response body is not empty")]
    public async Task GetProductById_ValidId_ReturnsNonEmptyBody()
    {
        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(TestProductId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("2.5 Response has all expected fields")]
    public async Task GetProductById_ValidId_HasAllExpectedFields()
    {
        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(TestProductId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.6 `id` in response matches requested ID")]
    public async Task GetProductById_ValidId_IdMatchesRequested()
    {
        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(TestProductId));

        response.Data!.Id.Should().Be(TestProductId);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
    [Category("Negative")]
    [Description("2.7 Get product by non-existent ID (maxId + 1) — status code 404")]
    public async Task GetProductById_NonExistentId_ReturnsNotFound()
    {
        var allProducts = await Get<List<ProductModelResponse>>(FakeStoreEndpoints.Products);
        var maxId = allProducts.Data!.Max(p => p.Id);
        var nonExistentId = maxId + 1;

        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for ID=0")]
    [Category("Negative")]
    [Description("2.8 Get product by ID = 0 — status code 404")]
    public async Task GetProductById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for negative IDs")]
    [Category("Negative")]
    [Description("2.9 Get product by negative ID (-1) — status code 404")]
    public async Task GetProductById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("2.10 Get product by ID = 5 returns valid product")]
    public async Task GetProductById_Id5_ReturnsValidProduct()
    {
        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(BoundaryProductId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(BoundaryProductId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("2.11 Get product by ID = 20 (last) returns valid product")]
    public async Task GetProductById_Id20_ReturnsValidProduct()
    {
        var response = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(LastProductId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(LastProductId);
    }

    [Test]
    [Category("Regression")]
    [Description("2.12 Get product by ID = 1 returns same product on repeated calls")]
    public async Task GetProductById_RepeatedCalls_ReturnSameProduct()
    {
        var response1 = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(TestProductId));
        var response2 = await Get<ProductModelResponse>(FakeStoreEndpoints.ProductsById, IdParam(TestProductId));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.Title.Should().Be(response2.Data!.Title);
        response1.Data!.Price.Should().Be(response2.Data!.Price);
    }
}
