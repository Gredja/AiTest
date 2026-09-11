using FluentAssertions;
using NUnit.Framework;
using TestAdapter;

namespace Ui.Tests;

[TestFixture]
[AllureNUnit]
[Category("Ui")]
public class DummyTests
{
    [Test]
    [Category("Smoke")]
    public void DummySmokeTest() =>
        true.Should().BeTrue("this is a dummy test to verify Allure integration works");

    [Test]
    [Ignore("This is a dummy ignored test to verify skipped tests appear in Allure")]
    public void DummyIgnoredTest() =>
        true.Should().BeFalse("this should never run");
}
