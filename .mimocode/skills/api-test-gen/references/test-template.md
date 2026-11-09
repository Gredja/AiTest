# Positive template — GetAll{Endpoint} test class

```csharp
using NUnit.Framework;
using RestSharp;
using Core.Models.{Service};
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static {ServiceParamHelperFullName};

namespace Api.{Service}.Tests;

[TestFixture]
[AllureNUnit]
[Category("{Service}")]
public class GetAll{Endpoint}Tests : {BaseClass}
{
    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAll{Endpoint}_ReturnsOk()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("1.2 Response matches expected model schema")]
    public async Task GetAll{Endpoint}_ResponseMatchesModel()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidFields();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Content-Type is application/json")]
    public async Task GetAll{Endpoint}_ContentTypeIsJson()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Category("Smoke")]
    [Description("1.4 Response body is not empty")]
    public async Task GetAll{Endpoint}_ReturnsNonEmptyList()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("1.5 Each item has valid required fields (via attributes)")]
    public async Task GetAll{Endpoint}_EachItemHasValidFields()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        foreach (var item in response.Data!)
        {
            item.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Performance")]
    [Description("1.6 Response time < 5 seconds")]
    public async Task GetAll{Endpoint}_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        stopwatch.Stop();
        AssertHelper.ShouldBeWithinResponseBudget(stopwatch.ElapsedMilliseconds);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.7 Returns expected count")]
    public async Task GetAll{Endpoint}_ReturnsExpectedCount()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(Expected{Endpoint}Count);  // const in Endpoints or test class
    }
}
```
