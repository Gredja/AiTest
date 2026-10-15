using AllureAdapter.Helpers;

namespace Api;

[SetUpFixture]
public class AllureGlobalSetup
{
    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        var resultsDir = AllureHelper.GetResultsDir();
        AllureSkippedTestWriter.WriteSkippedTests(typeof(AllureGlobalSetup).Assembly, resultsDir);
    }
}
