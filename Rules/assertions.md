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
- `response.ShouldHaveStatusCode(HttpStatusCode.OK)` — extension from `AssertHelper`
- `item.ShouldHaveValidFields()` — validates `[RequiredField]`, `[PositiveId]`, `[ValueRange]` attributes
- `response.Data!.ShouldMatchRequest(_testBody)` — compare request fields against response

## HTTP response assertions

- Check status code: `response.ShouldHaveStatusCode(HttpStatusCode.OK)`
- Check content type: `response.ContentType.Should().Contain("application/json")`
- Check response time: `stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxResponseTimeMs)`
- Check error message: `response.Data!.Message.Should().Be("Not Found")` — for 404 responses

## Request/Response comparison

- Use `AssertHelper.ShouldMatchRequest<TRequest, TResponse>()` after POST/PATCH
- Helper maps request properties to response by name (case-insensitive)
- Skips null values in request (only validates fields that were sent)
- Validate server-generated fields separately: `id`, `created_at`, `state`

## Notes

- `Assert.Multiple` not needed — FluentAssertions gives clear error messages
- Avoid `out _` inside `OnlyContain` lambdas (expression tree limitation)
