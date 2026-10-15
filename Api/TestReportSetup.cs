using Core.Logging;
using Core.Reporting;

[assembly: TestOutcome]

namespace Api;

// Duplicated in Api/ and E2E/ deliberately: [SetUpFixture] must live inside each test
// assembly - moving it to a referenced assembly breaks run-level setup discovery
[SetUpFixture]
public class TestReportSetup
{
    [OneTimeSetUp]
    public void PrepareRun() => TestRunReportGenerator.PrepareRun();

    [OneTimeTearDown]
    public void GenerateReport() => TestRunReportGenerator.Generate();
}
