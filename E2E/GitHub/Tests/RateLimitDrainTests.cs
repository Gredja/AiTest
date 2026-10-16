using Core.Models.GitHub;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.TestDataConstants;

namespace E2E.GitHub.Tests;

[TestFixture]
[AllureNUnit]
[Category("GitHubE2E")]
public class RateLimitDrainTests : GitHubE2ETestBase
{
    private const int DrainCallCount = 5;
    private const int QuotaHeadroom = 10;
    private const int AuthenticatedRateLimit = 5000;

    [Test]
    [Category("Regression")]
    [Description("E2E-6 Rate limit drain: consume calls → verify remaining decrements → verify reset is future")]
    public async Task RateLimitDrain_Consume_VerifyDecrement_VerifyReset()
    {
        var remainingFirst = 0;
        var remainingLast = 0;
        var resetTime = DateTimeOffset.MinValue;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var first = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, TestRepoParam());
            first.ShouldHaveStatusCode(HttpStatusCode.OK);

            var limitHeader = RateLimitHeaderValue(first, GitHubHeaders.RateLimitLimit);
            limitHeader.Should().NotBeNullOrWhiteSpace("counting response must carry the rate limit header");
            var limit = int.Parse(limitHeader);
            limit.Should().Be(AuthenticatedRateLimit,
                "authenticated primary quota must be 5000 per hour — OB §16");

            var remainingHeader = RateLimitHeaderValue(first, GitHubHeaders.RateLimitRemaining);
            remainingHeader.Should().NotBeNullOrWhiteSpace(RateLimitRemainingHeaderMessage);
            remainingFirst = int.Parse(remainingHeader);
            remainingFirst.Should().BeGreaterThan(DrainCallCount + QuotaHeadroom,
                "quota must have headroom for the drain budget — wait for the hourly reset");

            var last = first;
            for (var call = 1; call < DrainCallCount; call++)
            {
                last = await Get<RepositoryModelResponse>(GitHubEndpoints.ReposById, TestRepoParam());
                last.ShouldHaveStatusCode(HttpStatusCode.OK);
            }

            var remainingLastHeader = RateLimitHeaderValue(last, GitHubHeaders.RateLimitRemaining);
            remainingLastHeader.Should().NotBeNullOrWhiteSpace(RateLimitRemainingHeaderMessage);
            remainingLast = int.Parse(remainingLastHeader);

            var resetHeader = RateLimitHeaderValue(first, GitHubHeaders.RateLimitReset);
            resetHeader.Should().NotBeNullOrWhiteSpace("counting response must carry the reset timestamp");
            resetTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(resetHeader));

            if (remainingLast < remainingFirst)
            {
                break;
            }
        }

        remainingLast.Should().BeLessThan(remainingFirst,
            "every counting API call must decrement the remaining quota");
        (remainingFirst - remainingLast).Should().BeGreaterThanOrEqualTo(DrainCallCount - 1,
            "each of this test's drain calls must be accounted in the headers");
        resetTime.Should().BeAfter(DateTimeOffset.UtcNow, "reset must schedule the next quota window");
    }
}
