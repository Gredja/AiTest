using RestSharp;
using Core.Config;
using Core.Helpers;
using Core.Helpers.GitHub;
using Core.Models.GitHub;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.GitHub.RateLimit;

[TestFixture]
[AllureNUnit]
[Category("GitHub")]
public class GetRateLimitTests : GitHubTestBase
{
    [Test]
    [Category("HealthCheck")]
    [Description("6.1 GET /rate_limit returns 200 OK")]
    public async Task GetRateLimit_ReturnsOk()
    {
        var response = await Get<RateLimitModelResponse>(GitHubEndpoints.RateLimit);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("6.2 Rate limit remaining is positive")]
    public async Task GetRateLimit_RemainingIsPositive()
    {
        var response = await Get<RateLimitModelResponse>(GitHubEndpoints.RateLimit);

        response.Data!.Rate.Remaining.Should().BeGreaterThan(0);
    }

    [Test]
    [Category("Regression")]
    [Description("6.3 Rate limit reset is a future timestamp")]
    public async Task GetRateLimit_ResetIsFuture()
    {
        var response = await Get<RateLimitModelResponse>(GitHubEndpoints.RateLimit);

        var resetTime = DateTimeOffset.FromUnixTimeSeconds(response.Data!.Rate.Reset);
        resetTime.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Test]
    [Category("Performance")]
    [Description("6.4 Response time < 5 seconds")]
    public async Task GetRateLimit_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<RateLimitModelResponse>(GitHubEndpoints.RateLimit);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(TestConfig.MaxResponseTimeMs);
    }

    [Test]
    [Category("Regression")]
    [Description("6.5 Rate limit response has core and resources sections")]
    public async Task GetRateLimit_HasAllSections()
    {
        var response = await Get<RateLimitModelResponse>(GitHubEndpoints.RateLimit);

        response.Data.Should().NotBeNull();
        response.Data!.Resources.Should().NotBeNull();
        response.Data.Resources.Core.Should().NotBeNull();
        response.Data.Resources.Search.Should().NotBeNull();
        response.Data.Rate.Should().NotBeNull();
    }

    [Test]
    [Category("Regression")]
    [Description("6.6 Rate limit used is non-negative")]
    public async Task GetRateLimit_UsedIsNonNegative()
    {
        var response = await Get<RateLimitModelResponse>(GitHubEndpoints.RateLimit);

        response.Data!.Rate.Used.Should().BeGreaterThanOrEqualTo(0);
    }
}
