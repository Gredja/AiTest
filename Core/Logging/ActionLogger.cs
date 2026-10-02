using Serilog;
using Serilog.Core;

namespace Core.Logging;

internal static class ActionLogger
{
    private static readonly Lazy<Logger> _logger = new(CreateLogger);

    internal static ILogger Logger => _logger.Value;

    private static Logger CreateLogger()
    {
        var path = Path.Combine(
            TestRunWorkspace.Resolve(),
            $"actions-{DateTime.Now:yyyyMMdd-HHmmss}-{Environment.ProcessId}.log");

        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.With<TestNameEnricher>()
            .WriteTo.File(
                path,
                outputTemplate: "{Timestamp:HH:mm:ss.fff}|{Level:u3}|{TestName}|{Message:lj}{NewLine}",
                shared: true)
            .CreateLogger();

        AppDomain.CurrentDomain.ProcessExit += (_, _) => logger.Dispose();
        return logger;
    }
}
