using System.Text.Json;

namespace Core.Logging;

internal static class TestOutcomeWriter
{
    private static readonly Lazy<string> _logPath = new(() => Path.Combine(
        TestRunWorkspace.Resolve(),
        $"test-results-{DateTime.Now:yyyyMMdd-HHmmss}-{Environment.ProcessId}.log"));

    internal static void Write(TestOutcome outcome)
    {
        var line = JsonSerializer.Serialize(new
        {
            test = outcome.Name,
            status = outcome.Status,
            durationMs = outcome.DurationMilliseconds,
            message = outcome.Message,
            trace = outcome.Trace
        });

        File.AppendAllText(_logPath.Value, line + Environment.NewLine);
    }
}
