using NUnit.Framework;
using TestAdapter.Helpers;

[SetUpFixture]
public class AllureGlobalSetup
{
    [OneTimeSetUp]
    public void GlobalSetup() { }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        var resultsDir = AllureHelper.GetResultsDir();
        AllureSkippedTestWriter.WriteSkippedTests(typeof(AllureGlobalSetup).Assembly, resultsDir);
    }
}
