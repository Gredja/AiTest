using RestSharp;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.FakeStore.Carts;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetAllCartsTests : RequestHelper
{
    [Test]
    [Category("HealthCheck")]
    [Description("11.1 Status code is 200")]
    public async Task GetAllCarts_ReturnsOk()
    {
        var response = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("11.2 Response matches expected contract")]
    public async Task GetAllCarts_ResponseMatchesContract()
    {
        var response = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("11.3 Content-Type is application/json")]
    public async Task GetAllCarts_ContentTypeIsJson()
    {
        var response = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Smoke")]
    [Description("11.4 Response body is not empty")]
    public async Task GetAllCarts_ReturnsNonEmptyList()
    {
        var response = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Smoke")]
    [Description("11.5 Returns exactly 7 carts")]
    public async Task GetAllCarts_ReturnsExpectedCount()
    {
        var response = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(TestConfig.ExpectedCartCount);
    }

    [Test]
    [Category("Regression")]
    [Description("11.6 Each cart has valid fields")]
    public async Task GetAllCarts_EachCartHasValidFields()
    {
        var response = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);

        foreach (var cart in response.Data!)
        {
            cart.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Regression")]
    [Description("11.7 All cart IDs are unique")]
    public async Task GetAllCarts_AllIdsAreUnique()
    {
        var response = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);

        var ids = response.Data!.Select(c => c.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Test]
    [Category("Regression")]
    [Description("11.8 Each cart has non-empty products list")]
    public async Task GetAllCarts_EachCartHasProducts()
    {
        var response = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);

        foreach (var cart in response.Data!)
        {
            cart.Products.Should().NotBeEmpty();
        }
    }

    [Test]
    [Category("Performance")]
    [Description("11.9 Response time < 5 seconds")]
    public async Task GetAllCarts_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<CartModelResponse>>(FakeStoreEndpoints.Carts);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }
}
