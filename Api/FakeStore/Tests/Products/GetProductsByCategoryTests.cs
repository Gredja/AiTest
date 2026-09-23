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
public class GetProductsByCategoryTests : RequestHelper
{
    [Test]
    [Category("HealthCheck")]
    [Description("10.1 Status code is 200 for valid category")]
    public async Task GetProductsByCategory_ValidCategory_ReturnsOk()
    {
        var response = await Get<List<ProductModelResponse>>(
            $"/products/category/{TestConfig.TestCategoryName}");

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("10.2 Response matches expected contract")]
    public async Task GetProductsByCategory_ValidCategory_ResponseMatchesContract()
    {
        var response = await Get<List<ProductModelResponse>>(
            $"/products/category/{TestConfig.TestCategoryName}");

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("10.3 Content-Type is application/json")]
    public async Task GetProductsByCategory_ValidCategory_ContentTypeIsJson()
    {
        var response = await Get<List<ProductModelResponse>>(
            $"/products/category/{TestConfig.TestCategoryName}");

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("10.4 Response body is not empty")]
    public async Task GetProductsByCategory_ValidCategory_ReturnsNonEmptyList()
    {
        var response = await Get<List<ProductModelResponse>>(
            $"/products/category/{TestConfig.TestCategoryName}");

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("10.5 All items belong to requested category")]
    public async Task GetProductsByCategory_ValidCategory_AllItemsBelongToCategory()
    {
        var response = await Get<List<ProductModelResponse>>(
            $"/products/category/{TestConfig.TestCategoryName}");

        response.Data.Should().OnlyContain(p =>
            p.Category.Equals(TestConfig.TestCategoryName, StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    [Category("Regression")]
    [Description("10.6 Each item has valid fields")]
    public async Task GetProductsByCategory_ValidCategory_EachItemHasValidFields()
    {
        var response = await Get<List<ProductModelResponse>>(
            $"/products/category/{TestConfig.TestCategoryName}");

        foreach (var product in response.Data!)
        {
            product.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("10.7 Returns expected count for electronics")]
    public async Task GetProductsByCategory_Electronics_ReturnsExpectedCount()
    {
        var response = await Get<List<ProductModelResponse>>(
            $"/products/category/{TestConfig.TestCategoryName}");

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(TestConfig.ExpectedProductsInCategoryCount);
    }

    [Test]
    [Category("Negative")]
    [Description("10.8 Non-existent category returns empty list")]
    public async Task GetProductsByCategory_NonExistentCategory_ReturnsEmpty()
    {
        var response = await Get<List<ProductModelResponse>>(
            "/products/category/nonexistentcategory123");

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Performance")]
    [Description("10.9 Response time < 5 seconds")]
    public async Task GetProductsByCategory_ValidCategory_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<ProductModelResponse>>(
            $"/products/category/{TestConfig.TestCategoryName}");
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
