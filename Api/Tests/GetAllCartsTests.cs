using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Config;
using Api.Helpers;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.Tests;

[TestFixture]
[AllureNUnit]
public class GetAllCartsTests : RequestHelper
{
    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("1.1 Status code is 200")]
    public async Task GetAllCarts_ReturnsOk()
    {
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("1.2 Response body is not empty")]
    public async Task GetAllCarts_ReturnsNonEmptyList()
    {
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("1.3 Content-Type is application/json")]
    public async Task GetAllCarts_ContentTypeIsJson()
    {
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);

        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("1.4 Each item has `id` (integer)")]
    public async Task GetAllCarts_EachItemHasId()
    {
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);

        response.Data!.ShouldAllHaveValidCartIds();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("1.5 Each item has `userId` (integer)")]
    public async Task GetAllCarts_EachItemHasUserId()
    {
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);

        response.Data!.ShouldAllHaveValidUserId();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("1.6 Each item has `date` (string, not empty)")]
    public async Task GetAllCarts_EachItemHasDate()
    {
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);

        response.Data!.ShouldAllHaveValidDate();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("1.7 Each item has `products` (non-empty list)")]
    public async Task GetAllCarts_EachItemHasProducts()
    {
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);

        response.Data!.ShouldAllHaveValidProducts();
    }

    [Test]
    [Category("Performance")]
    [Category("Slow")]
    [Description("1.8 Response time < 5 seconds")]
    public async Task GetAllCarts_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(Endpoints.MaxResponseTimeMs);
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("1.9 Returns exactly 7 carts")]
    public async Task GetAllCarts_ReturnsExpectedCount()
    {
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(Endpoints.ExpectedCartCount);
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("1.10 Each product has `productId` and `quantity`")]
    public async Task GetAllCarts_EachProductHasRequiredFields()
    {
        var response = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);

        response.Data!.ShouldAllHaveValidProducts();
        response.Data!.SelectMany(c => c.Products).Should().OnlyContain(p => p.ProductId > 0 && p.Quantity >= 1);
    }
}
