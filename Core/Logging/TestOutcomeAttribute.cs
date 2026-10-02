using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Core.Logging;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class TestOutcomeAttribute : Attribute, ITestAction
{
    private long _startTicks;

    public ActionTargets Targets => ActionTargets.Test;

    public void BeforeTest(ITest test) => _startTicks = Environment.TickCount64;

    public void AfterTest(ITest test) => TestOutcomeWriter.Write(CreateOutcome(test));

    private TestOutcome CreateOutcome(ITest test)
    {
        var result = TestContext.CurrentContext.Result;
        var duration = Environment.TickCount64 - _startTicks;
        var status = result.Outcome.Status switch
        {
            TestStatus.Passed => "passed",
            TestStatus.Skipped => "skipped",
            _ => "failed"
        };

        return new TestOutcome(
            test.Name,
            status,
            duration,
            result.Message ?? string.Empty,
            result.StackTrace ?? string.Empty);
    }
}
