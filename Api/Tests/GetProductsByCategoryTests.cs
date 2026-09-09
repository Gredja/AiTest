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
[AllureEpic("API")]
[AllureFeature("Products")]
[AllureStory("Get Products By Category")]
public class GetProductsByCategoryTests : RequestHelper
{
    private static List<RequestDictionaryModel> CategoryParam(string category) =>
        new() { new() { Type = "UrlSegment", Key = "category", Value = category } };

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("4.1 Get products by category — status code 200")]
    public async Task GetProductsByCategory_ReturnsOk()
    {
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("4.2 Response body is not empty")]
    public async Task GetProductsByCategory_ReturnsNonEmptyList()
    {
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.3 Content-Type is application/json")]
    public async Task GetProductsByCategory_ContentTypeIsJson()
    {
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));

        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.4 Each item has `id` (positive integer)")]
    public async Task GetProductsByCategory_EachItemHasId()
    {
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));

        response.Data!.ShouldAllHaveValidProductIds();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.5 Each item has `title` (non-empty string)")]
    public async Task GetProductsByCategory_EachItemHasTitle()
    {
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));

        response.Data!.ShouldAllHaveValidTitle();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.6 Each item has `price` (>= 0)")]
    public async Task GetProductsByCategory_EachItemHasPrice()
    {
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));

        response.Data!.ShouldAllHaveValidPrice();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.7 Each item has `rating` (rate 0-5)")]
    public async Task GetProductsByCategory_EachItemHasRating()
    {
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));

        response.Data!.ShouldAllHaveValidRating();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.8 All items belong to the requested category")]
    public async Task GetProductsByCategory_AllItemsMatchCategory()
    {
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));

        response.Data!.Should().OnlyContain(p =>
            p.Category == Endpoints.TestCategoryName,
            $"all products should belong to '{Endpoints.TestCategoryName}' category");
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("4.9 Returns expected count for electronics (6)")]
    public async Task GetProductsByCategory_ReturnsExpectedCount()
    {
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(Endpoints.ExpectedProductsInCategoryCount);
    }

    [Test]
    [Category("Performance")]
    [Category("Slow")]
    [Description("4.10 Response time < 5 seconds")]
    public async Task GetProductsByCategory_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<ProductModel>>(
            Endpoints.ProductsByCategory, Method.Get, CategoryParam(Endpoints.TestCategoryName));
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(Endpoints.MaxResponseTimeMs);
    }
}
