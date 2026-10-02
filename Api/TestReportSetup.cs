using Core.Logging;

[assembly: TestOutcome]

namespace Api;

[SetUpFixture]
public class TestReportSetup
{
    [OneTimeTearDown]
    public void GenerateReport() => TestRunReportGenerator.Generate();
}
