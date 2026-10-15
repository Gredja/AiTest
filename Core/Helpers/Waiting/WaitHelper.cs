using System.Diagnostics;

namespace Core.Helpers;

public static class WaitHelper
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan _defaultInterval = TimeSpan.FromSeconds(2);

    public static async Task<WaitResult<T>> WaitUntilAsync<T>(
        Func<Task<T>> action,
        Func<T, bool> condition,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        var effectiveTimeout = timeout ?? _defaultTimeout;
        var effectiveInterval = interval ?? _defaultInterval;

        var stopwatch = Stopwatch.StartNew();
        var attempts = 0;
        T? lastValue = default;

        while (true)
        {
            lastValue = await action();
            attempts++;

            if (condition(lastValue))
            {
                return Result(true, stopwatch.Elapsed, attempts, lastValue);
            }

            if (stopwatch.Elapsed >= effectiveTimeout)
            {
                return Result(false, stopwatch.Elapsed, attempts, lastValue);
            }

            var remaining = effectiveTimeout - stopwatch.Elapsed;
            var delay = remaining < effectiveInterval ? remaining : effectiveInterval;

            await Task.Delay(delay);
        }
    }

    private static WaitResult<T> Result<T>(bool isSuccess, TimeSpan elapsed, int attempts, T? lastValue) =>
        new()
        {
            IsSuccess = isSuccess,
            Elapsed = elapsed,
            Attempts = attempts,
            LastValue = lastValue
        };
}
