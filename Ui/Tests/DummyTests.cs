using NUnit.Framework;
using TestAdapter;

namespace Ui.Tests;

[TestFixture]
[AllureNUnit]
public class DummyTests
{
    [Test]
    public void DummySmokeTest()
    {
        Assert.Pass("This is a dummy test to verify Allure integration works");
    }

    [Test]
    [Ignore("This is a dummy ignored test to verify skipped tests appear in Allure")]
    public void DummyIgnoredTest()
    {
        Assert.Fail("This should never run");
    }
}
