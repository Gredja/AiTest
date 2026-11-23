using NUnit.Framework;
using Serilog.Core;
using Serilog.Events;

namespace Core.Logging;

internal sealed class TestNameEnricher : ILogEventEnricher
{
    // Shared contract with ActionLogger's output template ({TestName} slot)
    internal const string TestNameProperty = "TestName";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var testName = TestContext.CurrentContext.Test?.FullName ?? string.Empty;
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(TestNameProperty, testName));
    }
}
