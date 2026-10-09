using Core.Logging;
using Core.Reporting;

[assembly: TestOutcome]

namespace Ui;

// Duplicated per test assembly deliberately: [SetUpFixture] must live inside the
// assembly it configures — moving it to a referenced assembly breaks discovery (audit P2.3)
[SetUpFixture]
public class TestReportSetup
{
    [OneTimeSetUp]
    public void PrepareRun() => TestRunReportGenerator.PrepareRun();

    [OneTimeTearDown]
    public void GenerateReport() => TestRunReportGenerator.Generate();
}
