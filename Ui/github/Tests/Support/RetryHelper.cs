using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Github.Tests.Support;

// Transient Playwright failures (hydration swallow, open-menu overlay, destroyed context)
// are swallowed so a single flaky click cannot kill the wait
internal static class RetryHelper
{
    internal static async Task<WaitResult<bool>> UntilAsync(
        Func<Task<bool>> isDone,
        Func<Task> attempt,
        TimeSpan? timeout = null,
        TimeSpan? interval = null) =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                try
                {
                    if (await isDone())
                    {
                        return true;
                    }

                    await attempt();
                }
                catch (PlaywrightException)
                {
                    // transient — the next poll re-reads the state
                }
                catch (TimeoutException)
                {
                    // transient — the next poll re-reads the state
                }

                return false;
            },
            done => done,
            timeout,
            interval);

    internal static Task<WaitResult<bool>> UntilAsync(
        Func<bool> isDone,
        Func<Task> attempt,
        TimeSpan? timeout = null,
        TimeSpan? interval = null) =>
        UntilAsync(() => Task.FromResult(isDone()), attempt, timeout, interval);
}
