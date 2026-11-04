using Core.Helpers;
using FluentAssertions;

namespace Api.CoreHelpers;

[TestFixture]
[AllureNUnit]
[Category("Core")]
public class AssertHelperMatchTests
{
    [Test]
    [Category("Regression")]
    public void ShouldMatchRequest_EqualValues_DoesNotThrow()
    {
        var request = new Request { Title = "title", Id = 10 };
        var response = new Response { Title = "title", Id = 10 };

        var act = () => response.ShouldMatchRequest(request);

        act.Should().NotThrow();
    }

    [Test]
    [Category("Negative")]
    public void ShouldMatchRequest_StringMismatch_Throws()
    {
        var request = new Request { Title = "expected", Id = 10 };
        var response = new Response { Title = "actual", Id = 10 };

        var act = () => response.ShouldMatchRequest(request);

        act.Should().Throw<AssertionException>();
    }

    [Test]
    [Category("Negative")]
    public void ShouldMatchRequest_MissingResponseProperty_Throws()
    {
        var request = new Request { Title = "title", Extra = "value" };
        var response = new Response { Title = "title" };

        var act = () => response.ShouldMatchRequest(request);

        act.Should().Throw<AssertionException>();
    }

    [Test]
    [Category("Regression")]
    public void ShouldMatchRequest_NullRequestFields_AreSkipped()
    {
        var request = new Request { Title = null, Id = null, Extra = null };
        var response = new Response { Title = "anything" };

        var act = () => response.ShouldMatchRequest(request);

        act.Should().NotThrow();
    }

    [Test]
    [Category("Regression")]
    public void ShouldMatchRequest_EqualNestedPayload_DoesNotThrow()
    {
        var request = new Request { Payload = new Payload { Count = 3 } };
        var response = new Response { Payload = new Payload { Count = 3 } };

        var act = () => response.ShouldMatchRequest(request);

        act.Should().NotThrow();
    }

    [Test]
    [Category("Negative")]
    public void ShouldMatchRequest_NestedPayloadMismatch_Throws()
    {
        var request = new Request { Payload = new Payload { Count = 3 } };
        var response = new Response { Payload = new Payload { Count = 4 } };

        var act = () => response.ShouldMatchRequest(request);

        act.Should().Throw<AssertionException>();
    }

    private sealed class Request
    {
        public string? Title { get; init; }
        public int? Id { get; init; }
        public string? Extra { get; init; }
        public Payload? Payload { get; init; }
    }

    private sealed class Response
    {
        public string? Title { get; init; }
        public int? Id { get; init; }
        public Payload? Payload { get; init; }
    }

    private sealed class Payload
    {
        public int Count { get; init; }
    }
}
