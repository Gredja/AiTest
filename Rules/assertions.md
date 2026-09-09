# Rules: Assertions

## Library

FluentAssertions (not NUnit Assert).

## Key patterns

- `x.Should().Be(y)` — equals
- `x.Should().NotBeNull()` — not null
- `x.Should().NotBeNullOrWhiteSpace()` — string not null/empty
- `x.Should().BeGreaterThan(y)` — comparison
- `x.Should().BeInRange(a, b)` — range check
- `collection.Should().OnlyContain(p => ...)` — all items match predicate
- `x.Should().NotBeEmpty()` — collection not empty

## Notes

- `Assert.Multiple` not needed — FluentAssertions gives clear error messages
- Avoid `out _` inside `OnlyContain` lambdas (expression tree limitation)
