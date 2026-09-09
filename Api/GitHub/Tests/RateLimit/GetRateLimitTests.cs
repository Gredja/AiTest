using NUnit.Framework;
using RestSharp;
using Core.Config;
using Core.Helpers;
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
        var response = await Get<object>(GitHubEndpoints.RateLimit, Method.Get);

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Regression")]
    [Description("6.2 Response has rate limit remaining > 0")]
    public async Task GetRateLimit_RemainingIsPositive()
    {
        var response = await Get<object>(GitHubEndpoints.RateLimit, Method.Get);

        var remaining = response.Headers!.FirstOrDefault(h => h.Name == "X-RateLimit-Remaining");
        remaining.Should().NotBeNull();
        int.Parse(remaining!.Value!.ToString()!).Should().BeGreaterThan(0);
    }

    [Test]
    [Category("Regression")]
    [Description("6.3 Rate limit reset is a future timestamp")]
    public async Task GetRateLimit_ResetIsFuture()
    {
        var response = await Get<object>(GitHubEndpoints.RateLimit, Method.Get);

        var reset = response.Headers!.FirstOrDefault(h => h.Name == "X-RateLimit-Reset");
        reset.Should().NotBeNull();
        var resetTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(reset!.Value!.ToString()!));
        resetTime.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Test]
    [Category("Performance")]
    [Description("6.4 Response time < 5 seconds")]
    public async Task GetRateLimit_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Get<object>(GitHubEndpoints.RateLimit, Method.Get);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(GitHubEndpoints.MaxResponseTimeMs);
    }
}
