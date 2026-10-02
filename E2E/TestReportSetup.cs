using Core.Logging;

[assembly: TestOutcome]

namespace E2E;

[SetUpFixture]
public class TestReportSetup
{
    [OneTimeTearDown]
    public void GenerateReport() => TestRunReportGenerator.Generate();
}
