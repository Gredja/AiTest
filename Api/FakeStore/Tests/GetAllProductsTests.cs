using NUnit.Framework;
using RestSharp;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.FakeStore.Tests;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetAllProductsTests : RequestHelper
{
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

        response.ContentType.Should().Contain("application/json");
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
}
