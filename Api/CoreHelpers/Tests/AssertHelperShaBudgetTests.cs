using Core.Config;
using Core.Helpers;
using FluentAssertions;

namespace Api.CoreHelpers;

[TestFixture]
[AllureNUnit]
[Category("Core")]
public class AssertHelperShaBudgetTests
{
    private const int ShaLength = 40;

    [Test]
    [Category("Regression")]
    public void IsGitSha_LowercaseHex40_IsTrue()
    {
        var sha = new string('a', ShaLength);

        sha.IsGitSha().Should().BeTrue();
    }

    [Test]
    [Category("Regression")]
    public void IsGitSha_Uppercase_IsFalse()
    {
        var sha = new string('A', ShaLength);

        sha.IsGitSha().Should().BeFalse();
    }

    [Test]
    [Category("Regression")]
    public void IsGitSha_LengthAbove40_IsFalse()
    {
        var sha = new string('a', ShaLength + 1);

        sha.IsGitSha().Should().BeFalse();
    }

    [Test]
    [Category("Regression")]
    public void IsGitSha_NonHexCharacters_IsFalse()
    {
        var sha = new string('z', ShaLength);

        sha.IsGitSha().Should().BeFalse();
    }

    [Test]
    [Category("Regression")]
    public void ShouldHaveValidSha_ValidSha_DoesNotThrow()
    {
        var sha = new string('0', ShaLength);

        var act = () => sha.ShouldHaveValidSha();

        act.Should().NotThrow();
    }

    [Test]
    [Category("Negative")]
    public void ShouldHaveValidSha_Uppercase_Throws()
    {
        var sha = new string('A', ShaLength);

        var act = () => sha.ShouldHaveValidSha();

        act.Should().Throw<AssertionException>();
    }

    [Test]
    [Category("Performance")]
    public void ShouldBeWithinResponseBudget_WithinBudget_DoesNotThrow()
    {
        var elapsed = TestConfig.MaxResponseTimeMs - 1;

        var act = () => AssertHelper.ShouldBeWithinResponseBudget(elapsed);

        act.Should().NotThrow();
    }

    [Test]
    [Category("Performance")]
    public void ShouldBeWithinResponseBudget_AtBudgetBoundary_IsInconclusive()
    {
        var elapsed = TestConfig.MaxResponseTimeMs;

        var act = () => AssertHelper.ShouldBeWithinResponseBudget(elapsed);

        act.Should().Throw<InconclusiveException>();
    }
}
