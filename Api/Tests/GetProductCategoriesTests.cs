using NUnit.Framework;
using RestSharp;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.Tests;

[TestFixture]
[AllureNUnit]
public class GetProductCategoriesTests : RequestHelper
{
    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("3.1 Status code is 200")]
    public async Task GetProductCategories_ReturnsOk()
    {
        var response = await Get<List<string>>(Endpoints.ProductsCategories, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("3.2 Response body is not empty")]
    public async Task GetProductCategories_ReturnsNonEmptyList()
    {
        var response = await Get<List<string>>(Endpoints.ProductsCategories, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("3.3 Content-Type is application/json")]
    public async Task GetProductCategories_ContentTypeIsJson()
    {
        var response = await Get<List<string>>(Endpoints.ProductsCategories, Method.Get);

        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("3.4 Each item is non-empty string")]
    public async Task GetProductCategories_EachItemIsNonEmptyString()
    {
        var response = await Get<List<string>>(Endpoints.ProductsCategories, Method.Get);

        response.Data!.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c));
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("3.5 Returns exactly 4 categories")]
    public async Task GetProductCategories_ReturnsExpectedCount()
    {
        var response = await Get<List<string>>(Endpoints.ProductsCategories, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(Endpoints.ExpectedCategoryCount);
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("3.6 Contains expected category values")]
    public async Task GetProductCategories_ContainsExpectedValues()
    {
        var response = await Get<List<string>>(Endpoints.ProductsCategories, Method.Get);

        response.Data!.Should().Contain("electronics");
        response.Data!.Should().Contain("jewelery");
        response.Data!.Should().Contain("men's clothing");
        response.Data!.Should().Contain("women's clothing");
    }

    [Test]
    [Category("Performance")]
    [Category("Slow")]
    [Description("3.7 Response time < 5 seconds")]
    public async Task GetProductCategories_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<string>>(Endpoints.ProductsCategories, Method.Get);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(Endpoints.MaxResponseTimeMs);
    }
}
