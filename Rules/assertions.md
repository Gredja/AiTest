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
- `response.ShouldHaveError(HttpStatusCode.NotFound, GitHubErrors.NotFound)` — status + error body present + message readable + message equals the documented OB value (message constants live in `Core/Config/GitHubErrors.cs`); returns the parsed `ErrorMessageModelResponse` for follow-up asserts
- `item.ShouldHaveValidContract()` — JSON serializable + validates `[RequiredField]`, `[PositiveId]`, `[ValueRange]` attributes (use for ContractCheck)
- `item.ShouldHaveValidFields()` — validates attributes only (use when JSON serialization is already covered)
- `response.Data!.ShouldMatchRequest(_testBody)` — compare request fields against response

## HTTP response assertions

- Check status code: `response.ShouldHaveStatusCode(HttpStatusCode.OK)`
- Check content type: `response.ContentType.Should().Contain("application/json")`
- Check response time: `stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxResponseTimeMs)`
- Check error message: `response.ShouldHaveError(status, expectedMessage)` — for negative tests where OB documents the error text (status + readable body + documented message in one call); status-only `ShouldHaveStatusCode` is the fallback when the service has no documented message

## JSON response parsing

- Always parse JSON responses into typed models — never use `JsonElement` + `TryGetProperty` in tests
- Create a model class for every JSON response structure (e.g. `RateLimitModel`, `RateLimitSection`)
- Models live in `Core/Models/{Service}/` with proper nesting for nested JSON objects
- Use `response.Data!.Property` to assert — not raw JSON navigation

## Request/Response comparison

- Use `AssertHelper.ShouldMatchRequest<TRequest, TResponse>()` after POST/PATCH
- Helper maps request properties to response by name (case-insensitive)
- Skips null values in request (only validates fields that were sent)
- Comparison semantics (inside the helper): scalars/strings → `Be`; collections and nested objects → structural comparison (`BeEquivalentTo`) — plain `Be` on a collection is reference equality and fails every time; never hand-roll either variant in tests
- **Documented exception — nested-shape responses**: when the wire response nests a request scalar (create-ref: request `Sha` → response `object.sha`; create-PR: request `Head` → response `head.ref` object), the helper cannot map by name — assert those fields explicitly and note it at the call site; still use the helper everywhere it fits (PATCH echoes, create-comment)
- Validate server-generated fields separately: `id`, `created_at`, `state`

## Notes

- **Helpers collect, direct asserts stay hard** — `ShouldHaveValidContract`, `ShouldHaveValidFields` and `ShouldMatchRequest` gather every violation inside an `AssertionScope` and fail once with the full list (one rerun shows all bad fields); plain `x.Should()` in test bodies remains fail-fast; `Assert.Multiple` is not used
- Avoid `out _` inside `OnlyContain` lambdas (expression tree limitation)
