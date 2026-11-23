
namespace E2E;

[SetUpFixture]
public class AllureGlobalSetup
{
    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        var resultsDir = AllureHelper.GetResultsDirectory();
        AllureSkippedTestWriterHelper.WriteSkippedTests(typeof(AllureGlobalSetup).Assembly, resultsDir);
    }
}
