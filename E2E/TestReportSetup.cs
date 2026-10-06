using Core.Logging;
using Core.Reporting;

[assembly: TestOutcome]

namespace E2E;

[SetUpFixture]
public class TestReportSetup
{
    [OneTimeSetUp]
    public void PrepareRun() => TestRunReportGenerator.PrepareRun();

    [OneTimeTearDown]
    public void GenerateReport() => TestRunReportGenerator.Generate();
}
