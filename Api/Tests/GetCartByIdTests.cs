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
[AllureEpic("API")]
[AllureFeature("Carts")]
[AllureStory("Get Cart By ID")]
public class GetCartByIdTests : RequestHelper
{
    private static List<RequestDictionaryModel> CartIdParam(int id) =>
        new() { new() { Type = "UrlSegment", Key = "id", Value = id } };

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("2.1 Get cart by ID = 1 — status code 200")]
    public async Task GetCartById_ValidId_ReturnsOk()
    {
        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(Endpoints.TestCartId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("2.2 Response body is not empty")]
    public async Task GetCartById_ValidId_ReturnsNonEmptyBody()
    {
        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(Endpoints.TestCartId));

        response.ShouldBeOk();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("2.3 Response has all expected fields")]
    public async Task GetCartById_ValidId_HasAllExpectedFields()
    {
        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(Endpoints.TestCartId));

        response.Data!.ShouldHaveValidFields();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("2.4 `id` in response matches requested ID")]
    public async Task GetCartById_ValidId_IdMatchesRequested()
    {
        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(Endpoints.TestCartId));

        response.Data!.Id.Should().Be(Endpoints.TestCartId);
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("2.5 `userId` is positive integer")]
    public async Task GetCartById_ValidId_HasUserId()
    {
        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(Endpoints.TestCartId));

        response.Data!.UserId.Should().BeGreaterThan(0);
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("2.6 `products` is non-empty list")]
    public async Task GetCartById_ValidId_HasProducts()
    {
        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(Endpoints.TestCartId));

        response.Data!.Products.Should().NotBeEmpty();
        response.Data!.Products.Should().OnlyContain(p => p.ProductId > 0 && p.Quantity >= 1);
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("2.7 `date` is valid DateTime")]
    public async Task GetCartById_ValidId_HasDate()
    {
        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(Endpoints.TestCartId));

        response.Data!.Date.Should().BeAfter(default(DateTime));
    }

    [Test]
    [Category("Negative")]
    [Category("Slow")]
    [Description("2.8 Get cart by non-existent ID (maxId + 1) — returns null")]
    public async Task GetCartById_NonExistentId_ReturnsNull()
    {
        var allCarts = await Get<List<CartModel>>(Endpoints.Carts, Method.Get);
        var maxId = allCarts.Data!.Max(c => c.Id);
        var nonExistentId = maxId + 1;

        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeNull();
    }

    [Test]
    [Category("EdgeCase")]
    [Category("Slow")]
    [Description("2.9 Get cart by ID = 0 — returns null")]
    public async Task GetCartById_ZeroId_ReturnsNull()
    {
        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeNull();
    }

    [Test]
    [Category("EdgeCase")]
    [Category("Slow")]
    [Description("2.10 Get cart by negative ID (-1) — returns null")]
    public async Task GetCartById_NegativeId_ReturnsNull()
    {
        var response = await Get<CartModel>(Endpoints.CartsById, Method.Get, CartIdParam(-1));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeNull();
    }
}
