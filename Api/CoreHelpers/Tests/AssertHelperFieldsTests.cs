using Core.Attributes;
using Core.Helpers;
using FluentAssertions;

namespace Api.CoreHelpers;

[TestFixture]
[AllureNUnit]
[Category("Core")]
public class AssertHelperFieldsTests
{
    private const int MinimumScore = 1;
    private const int MaximumScore = 10;

    [Test]
    [Category("Regression")]
    public void ShouldHaveValidFields_ValidEntity_DoesNotThrow()
    {
        var entity = new Entity { Id = 1, Name = "value", Score = MaximumScore };

        var act = () => entity.ShouldHaveValidFields();

        act.Should().NotThrow();
    }

    [Test]
    [Category("Negative")]
    public void ShouldHaveValidFields_NullRequiredField_Throws()
    {
        var entity = new Entity { Id = 1, Name = null, Score = 5 };

        var act = () => entity.ShouldHaveValidFields();

        act.Should().Throw<AssertionException>();
    }

    [Test]
    [Category("Negative")]
    public void ShouldHaveValidFields_WhitespaceRequiredField_Throws()
    {
        var entity = new Entity { Id = 1, Name = "   ", Score = 5 };

        var act = () => entity.ShouldHaveValidFields();

        act.Should().Throw<AssertionException>();
    }

    [Test]
    [Category("Negative")]
    public void ShouldHaveValidFields_ZeroPositiveId_Throws()
    {
        var entity = new Entity { Id = 0, Name = "value", Score = 5 };

        var act = () => entity.ShouldHaveValidFields();

        act.Should().Throw<AssertionException>();
    }

    [Test]
    [Category("Negative")]
    public void ShouldHaveValidFields_ScoreBelowRange_Throws()
    {
        var entity = new Entity { Id = 1, Name = "value", Score = MinimumScore - 1 };

        var act = () => entity.ShouldHaveValidFields();

        act.Should().Throw<AssertionException>();
    }

    [Test]
    [Category("Negative")]
    public void ShouldHaveValidFields_ScoreAboveRange_Throws()
    {
        var entity = new Entity { Id = 1, Name = "value", Score = MaximumScore + 1 };

        var act = () => entity.ShouldHaveValidFields();

        act.Should().Throw<AssertionException>();
    }

    private sealed class Entity
    {
        [PositiveId]
        public int Id { get; init; }

        [RequiredField]
        public string? Name { get; init; }

        [ValueRange(MinimumScore, MaximumScore)]
        public int Score { get; init; }
    }
}
