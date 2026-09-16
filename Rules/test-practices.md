# Rules: Test Practices

## Test isolation

- Tests must be independent — no shared state between tests
- Order of execution must not matter
- Each test sets up its own data, doesn't rely on another test's side effects
- Clean up in teardown if tests create resources

## API testing patterns

- Use Given/When/Then structure (Arrange/Act/Assert)
- Check HTTP status code and response body separately — don't combine
- Verify content type when relevant
- For negative tests — verify error message or status, not just "not success"
- Prefer specific assertions over generic ones (check field values, not just "response is not null")

## Non-existent IDs

- Dynamic only: `GET all → maxId + 1`
- Never hardcode 999, 1000, or any static number
- If API returns 200 instead of 404 — use `[Ignore]` with explanation of the bug
- For negative tests with invalid types (0, negative, non-numeric) — use explicit values

## Request/Response comparison

- After POST/PATCH, compare request fields against response to verify API returned what was sent
- Use `AssertHelper.ShouldMatchRequest<TRequest, TResponse>()` — not inline assertions
- Helper maps request properties to response by name (case-insensitive), skips nulls
- Validate server-generated fields separately: `id` (positive), `created_at` (recent), `state` (expected value)

## E2E cleanup

- Every E2E test that creates a resource must delete it in teardown
- Cleanup order: delete comments before issues, delete branches before PRs, delete PRs before repos
- If cleanup fails, log warning but don't fail the test — resource can be manually cleaned
- Use sandbox repo for all write operations — never target production data

## Assertion helpers

- Use `ShouldHaveStatusCode()` for HTTP status checks — not `.Should().Be(HttpStatusCode.OK)`
- Use `ShouldHaveValidFields()` for attribute-based validation — not per-field assertions
- Use `ShouldMatchRequest()` for request/response comparison — not manual field mapping
- All helpers live in `Core/Helpers/AssertHelper.cs`
