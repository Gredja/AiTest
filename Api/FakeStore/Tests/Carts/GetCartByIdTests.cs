using RestSharp;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.FakeStore.Helpers.FakeStoreParamHelper;

namespace Api.FakeStore.Carts;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetCartByIdTests : RequestHelper
{
    private const int TestCartId = 1;
    private const int BoundaryCartId = 4;
    private const int LastCartId = 7;

    [Test]
    [Category("HealthCheck")]
    [Description("12.1 Get cart by ID = 1 — status code 200")]
    public async Task GetCartById_ValidId_ReturnsOk()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(TestCartId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("12.2 Response matches expected contract")]
    public async Task GetCartById_ResponseMatchesContract()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(TestCartId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("12.3 Response body is not null")]
    public async Task GetCartById_ValidId_ReturnsNonNull()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(TestCartId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("12.4 Response has all expected fields")]
    public async Task GetCartById_ValidId_HasAllExpectedFields()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(TestCartId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("12.5 `id` in response matches requested ID")]
    public async Task GetCartById_ValidId_IdMatchesRequested()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(TestCartId));

        response.Data!.Id.Should().Be(TestCartId);
    }

    [Test]
    [Category("Regression")]
    [Description("12.6 Cart has non-empty products list")]
    public async Task GetCartById_ValidId_HasProducts()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(TestCartId));

        response.Data!.Products.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("12.7 Get cart by ID = 4 returns valid cart")]
    public async Task GetCartById_BoundaryId_ReturnsValidCart()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(BoundaryCartId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(BoundaryCartId);
        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Regression")]
    [Description("12.8 Get cart by ID = 7 (last) returns valid cart")]
    public async Task GetCartById_LastId_ReturnsValidCart()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(LastCartId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(LastCartId);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
    [Category("Negative")]
    [Description("12.9 Get cart by non-existent ID (maxId + 1) — status code 404")]
    public async Task GetCartById_NonExistentId_ReturnsNotFound()
    {
        var allCarts = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);
        var maxId = allCarts.Data!.Max(c => c.Id);
        var nonExistentId = maxId + 1;

        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for ID=0")]
    [Category("Negative")]
    [Description("12.10 Get cart by ID = 0 — status code 404")]
    public async Task GetCartById_ZeroId_ReturnsNotFound()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for negative IDs")]
    [Category("Negative")]
    [Description("12.11 Get cart by negative ID (-1) — status code 404")]
    public async Task GetCartById_NegativeId_ReturnsNotFound()
    {
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Category("Regression")]
    [Description("12.12 Cart by ID = 1 returns same cart on repeated calls")]
    public async Task GetCartById_RepeatedCalls_ReturnSameCart()
    {
        var response1 = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(TestCartId));
        var response2 = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(TestCartId));

        response1.Data!.Id.Should().Be(response2.Data!.Id);
        response1.Data!.UserId.Should().Be(response2.Data!.UserId);
        response1.Data!.Products.Count.Should().Be(response2.Data!.Products.Count);
    }

    [Test]
    [Category("Performance")]
    [Description("12.13 Response time < 5 seconds")]
    public async Task GetCartById_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<CartModelResponse>(FakeStoreEndpoints.CartsById, IdParam(TestCartId));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
