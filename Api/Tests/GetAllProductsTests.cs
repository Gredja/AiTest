using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Config;
using Api.Helpers;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using TestAdapter;

namespace Api.Tests;

[TestFixture]
[AllureNUnit]
public class GetAllProductsTests : RequestHelper
{
    [Test]
    [Description("1.1 Status code is 200")]
    public async Task GetAllProducts_ReturnsOk()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Description("1.2 Response body is not empty")]
    public async Task GetAllProducts_ReturnsNonEmptyList()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Description("1.3 Response is a JSON array")]
    public async Task GetAllProducts_ResponseIsJsonArray()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.Data.Should().BeOfType<List<ProductModel>>();
    }

    [Test]
    [Description("1.4 Each item has `id` (integer)")]
    public async Task GetAllProducts_EachItemHasId()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.Data!.ShouldAllHaveValidId();
    }

    [Test]
    [Description("1.5 Each item has `title` (string, not empty)")]
    public async Task GetAllProducts_EachItemHasTitle()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.Data!.ShouldAllHaveValidTitle();
    }

    [Test]
    [Description("1.6 Each item has `price` (number, >= 0)")]
    public async Task GetAllProducts_EachItemHasPrice()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.Data!.ShouldAllHaveValidPrice();
    }

    [Test]
    [Description("1.7 Each item has `description` (string)")]
    public async Task GetAllProducts_EachItemHasDescription()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.Data!.ShouldAllHaveValidDescription();
    }

    [Test]
    [Description("1.8 Each item has `category` (string, not empty)")]
    public async Task GetAllProducts_EachItemHasCategory()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.Data!.ShouldAllHaveValidCategory();
    }

    [Test]
    [Description("1.9 Each item has `image` (string, valid URL)")]
    public async Task GetAllProducts_EachItemHasImage()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.Data!.ShouldAllHaveValidImage();
    }

    [Test]
    [Description("1.10 Each item has `rating` (object with `rate` 0-5, `count` >= 0)")]
    public async Task GetAllProducts_EachItemHasRating()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.Data!.ShouldAllHaveValidRating();
    }

    [Test]
    [Description("1.11 Response time < 5 seconds")]
    public async Task GetAllProducts_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(Endpoints.MaxResponseTimeMs);
    }

    [Test]
    [Description("1.12 Returns exactly 20 products")]
    public async Task GetAllProducts_ReturnsExpectedCount()
    {
        var response = await Get<List<ProductModel>>(Endpoints.Products, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(Endpoints.ExpectedProductCount);
    }
}
