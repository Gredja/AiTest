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
