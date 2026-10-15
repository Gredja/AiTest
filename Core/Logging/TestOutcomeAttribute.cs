using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Core.Logging;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class TestOutcomeAttribute : Attribute, ITestAction
{
    // One attribute instance serves the whole assembly — parallel tests must not share start ticks
    private static readonly AsyncLocal<long> _startTicks = new();

    public ActionTargets Targets => ActionTargets.Test;

    public void BeforeTest(ITest test) => _startTicks.Value = Environment.TickCount64;

    public void AfterTest(ITest test) => TestOutcomeWriter.Write(CreateOutcome(test));

    private TestOutcome CreateOutcome(ITest test)
    {
        var result = TestContext.CurrentContext.Result;
        var duration = Environment.TickCount64 - _startTicks.Value;
        var status = result.Outcome.Status switch
        {
            TestStatus.Passed => TestStatusNames.Passed,
            TestStatus.Skipped => TestStatusNames.Skipped,
            _ => TestStatusNames.Failed
        };

        return new TestOutcome(
            test.Name,
            status,
            duration,
            result.Message ?? string.Empty,
            result.StackTrace ?? string.Empty);
    }
}
