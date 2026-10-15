using NUnit.Framework;
using Serilog.Core;
using Serilog.Events;

namespace Core.Logging;

internal sealed class TestNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var testName = TestContext.CurrentContext.Test?.FullName ?? string.Empty;
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TestName", testName));
    }
}
