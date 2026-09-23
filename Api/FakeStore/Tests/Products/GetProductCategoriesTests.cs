using RestSharp;
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
public class GetProductCategoriesTests : RequestHelper
{
    [Test]
    [Category("HealthCheck")]
    [Description("9.1 Status code is 200")]
    public async Task GetProductCategories_ReturnsOk()
    {
        var response = await Get<List<string>>(FakeStoreEndpoints.ProductsCategories);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("9.2 Response is non-empty list of strings")]
    public async Task GetProductCategories_ResponseMatchesContract()
    {
        var response = await Get<List<string>>(FakeStoreEndpoints.ProductsCategories);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Smoke")]
    [Description("9.3 Content-Type is application/json")]
    public async Task GetProductCategories_ContentTypeIsJson()
    {
        var response = await Get<List<string>>(FakeStoreEndpoints.ProductsCategories);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("9.4 Response body is not empty")]
    public async Task GetProductCategories_ReturnsNonEmptyList()
    {
        var response = await Get<List<string>>(FakeStoreEndpoints.ProductsCategories);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("9.5 Returns exactly 4 categories")]
    public async Task GetProductCategories_ReturnsExpectedCount()
    {
        var response = await Get<List<string>>(FakeStoreEndpoints.ProductsCategories);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(TestConfig.ExpectedCategoryCount);
    }

    [Test]
    [Category("Regression")]
    [Description("9.6 Each category is non-empty string")]
    public async Task GetProductCategories_EachCategoryIsValid()
    {
        var response = await Get<List<string>>(FakeStoreEndpoints.ProductsCategories);

        response.Data.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c));
    }

    [Test]
    [Category("Regression")]
    [Description("9.7 All category names are unique")]
    public async Task GetProductCategories_AllCategoriesAreUnique()
    {
        var response = await Get<List<string>>(FakeStoreEndpoints.ProductsCategories);

        response.Data.Should().NotBeNull();
        response.Data!.Distinct().Should().HaveCount(response.Data.Count);
    }

    [Test]
    [Category("Performance")]
    [Description("9.8 Response time < 5 seconds")]
    public async Task GetProductCategories_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<string>>(FakeStoreEndpoints.ProductsCategories);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
