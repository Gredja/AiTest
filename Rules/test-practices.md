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

## Document sync

When adding or changing endpoints, keep these documents in sync:
- `documentation/{Service}ObservableBehaviour.md` — source of truth for test design (fields, types, negatives)
- `documentation/{Service}TestPlan.md` — coverage tracking (which endpoints have tests, status)
- `Rules/*.md` — shared rules for all services (assertions, patterns, cleanup)
- If Observable Behaviour changes → update Test Plan status; if Test Plan adds endpoints → update Observable Behaviour

## Seed methodology for test generation

When generating tests for an endpoint, use the Seed → Expand → Review approach:

**Step 1: Write 5 seeds** (1 sentence each)
- 2 happy path variations (different input combinations)
- 2 known failure modes (different error conditions)
- 1 edge case (boundary, special chars, or unusual input)

**Step 2: Expand via AI** — for each seed generate variations and output as a table:

| # | Case | Category | Priority | Source seed |
|---|------|----------|----------|-------------|
| 1 | Happy: title only → 201, state="open" | smoke | 1 | Seed 1 |
| 2 | Happy: title + body + labels → 201, all fields in response | critical-path | 1 | Seed 1 |
| 3 | Happy: generated id > 0, created_at within 1 min | regression | 1 | Seed 1 |
| 4 | Happy: ShouldMatchRequest for title and body | regression | 1 | Seed 1 |
| 5 | Negative: no auth → 401 | critical-path | 1 | Seed 3 |
| 6 | Negative: empty title → 422 | critical-path | 1 | Seed 3 |
| 7 | Negative: invalid token → 401 | regression | 2 | Seed 3 |
| 8 | Negative: wrong scope → 403 | regression | 2 | Seed 4 |
| 9 | Negative: non-existent repo → 404 | regression | 1 | Seed 4 |
| 10 | Negative: missing title field → 422 | critical-path | 1 | Seed 4 |
| 11 | Edge: title = 65536 chars → 201 or 422 | edge | 2 | Seed 5 |
| 12 | Edge: special chars in body (markdown/HTML) → 201 | edge | 2 | Seed 5 |
| 13 | Edge: double-click POST sends only one request | edge | 1 | Seed 2 |

Categories: `smoke` | `critical-path` | `regression` | `edge` | `negative`
Priority: `1` = must have, `2` = nice to have

**Step 3: Review and clean**
- Delete clearly wrong cases
- Deduplicate near-duplicates
- Re-tag mis-categorised tests
- **Check for magic numbers/strings** — extract to constants in `{Service}Endpoints.cs` before writing test code (see `Rules/code.md`, `Rules/code-style.md`)

**Step 4: Enforce negative floor** — minimum 5 negatives per endpoint
- If fewer than 5: generate more, each exercising a different failure mode
- Failure modes: no auth, invalid token, wrong scope, missing required field, non-existent resource, invalid value, boundary value

**Step 5: Save** — target ~15-20 unique test cases per endpoint

**Expected yield per endpoint:** ~8-14 tests (5-6 happy, 5-6 negative, 2-3 edge)
**Never:** less than 3 tests per endpoint, less than 5 negatives per endpoint

**Don't duplicate attribute checks:** `ShouldHaveValidFields()` covers `[RequiredField]`, `[PositiveId]`, `[ValueRange]`. Per-field assertions only for things attributes CAN'T cover: unique IDs, boundary values, idempotency, cross-field invariants, category counts.
