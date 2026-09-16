using RestSharp;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.FakeStore.Products;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetAllProductsTests : RequestHelper
{
    private const double RatingMin = 0;
    private const double RatingMax = 5;
    private const int ExpectedCategoryCount = 4;
    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAllProducts_ReturnsOk()
    {
        var response = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.2 Response body is not empty")]
    public async Task GetAllProducts_ReturnsNonEmptyList()
    {
        var response = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Content-Type is application/json")]
    public async Task GetAllProducts_ContentTypeIsJson()
    {
        var response = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Regression")]
    [Description("1.4 Each item has valid required fields (via attributes)")]
    public async Task GetAllProducts_EachItemHasValidFields()
    {
        var response = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);

        foreach (var product in response.Data!)
        {
            product.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Performance")]
    [Description("1.5 Response time < 5 seconds")]
    public async Task GetAllProducts_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(FakeStoreEndpoints.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.6 Returns exactly 20 products")]
    public async Task GetAllProducts_ReturnsExpectedCount()
    {
        var response = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(FakeStoreEndpoints.ExpectedProductCount);
    }

    [Test]
    [Category("Regression")]
    [Description("1.7 Each product has rating with rate between 0 and 5")]
    public async Task GetAllProducts_EachProductHasValidRating()
    {
        var response = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);

        response.Data.Should().OnlyContain(p => p.Rating.Rate >= RatingMin && p.Rating.Rate <= RatingMax);
    }

    [Test]
    [Category("Regression")]
    [Description("1.8 All product IDs are unique")]
    public async Task GetAllProducts_AllIdsAreUnique()
    {
        var response = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);

        var ids = response.Data!.Select(p => p.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("1.9 Products contain all categories")]
    public async Task GetAllProducts_ContainsAllCategories()
    {
        var response = await Get<List<ProductModel>>(FakeStoreEndpoints.Products, Method.Get);

        var categories = response.Data!.Select(p => p.Category).Distinct().ToList();
        categories.Should().HaveCount(ExpectedCategoryCount);
    }
}
