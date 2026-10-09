# Rules: Test Practices

## Test isolation

- Tests must be independent — no shared state between tests
- Order of execution must not matter
- Each test sets up its own data, doesn't rely on another test's side effects
- Clean up in teardown if tests create resources
- **Supported parallelism: `NumberOfTestWorkers = 1` (default)** — higher values are experimental: shared guarantee data (same issue #5, baseline counts) and set-diff `+1` invariants can flake across parallel fixtures
- **Setup requests assert their status** — every GET/POST inside `[OneTimeSetUp]` gets `ShouldHaveStatusCode(...)` immediately after the call. A failed setup must fail with a clear status assertion, not a `NullReferenceException` mid-fixture (kills CI diagnostics)
- **Teardown/cleanup requests do NOT assert status** — `[OneTimeTearDown]` cleanup runs under the shared `RunCleanupAsync` (try/catch + warning, see "E2E cleanup"): a status assertion there would fail the run for a resource that can be cleaned manually and would mask the original test failure. Log the status code in the warning instead; never dereference the cleanup response body
- **Status check before every `.Data` dereference** — not only in setups: when a test body reads `.Data!` or aggregates off a response, assert the status first; a 500 must fail as "expected 200, got 500", not as an NRE
- **Aggregates need data first** — `Max`/`First` anywhere (setup OR test body): status-assert first, then `NotBeEmpty(...)` whose message points at Entry Criteria (`documentation/GitHubTestingStructure.md`); a bare `InvalidOperationException: Sequence contains no elements` explains nothing

## API testing patterns

- **Test classes contain only tests** — `[Test]` methods, `[OneTimeSetUp]`/`[OneTimeTearDown]` lifecycle, and test data (constants, state fields). No helper methods inside `*Tests.cs`: shared request/param builders live in base classes (`GitHubTestBase` → `PullParams`, `MercyPreviewRepoParams`) or param helpers; when 2+ test classes grow the same private builder — extract it to the base, never duplicate
- **`[OneTimeTearDown]` is the LAST member of the test class** — after all `[Test]` methods; fields and `[OneTimeSetUp]` stay at the top
- Use Given/When/Then structure (Arrange/Act/Assert)
- Check HTTP status code and response body separately — don't combine
- Verify content type when relevant
- For negative tests — verify error message or status, not just "not success"
- Prefer specific assertions over generic ones (check field values, not just "response is not null")

## Non-existent IDs

- Dynamic only: `GET all → maxId + 1`
- **When the same run writes concurrently** (solution run: Api + E2E together) — `maxId + 1` is racy: a parallel E2E write can create exactly that number and flip an expected 404 into 200. Use `maxId + Offset` instead (GitHub: `GitHubEndpoints.NonExistentIdOffset = 100`); stateless mocks (JSONPlaceholder) keep `maxId + 1` — nothing can create there
- Never hardcode 999, 1000, or any static number
- If API returns 200 instead of 404 — use `[Ignore]` with explanation of the bug
- For negative tests with invalid types (0, negative, non-numeric) — use explicit values
- **Volatile external entities as test data** (live branch names, temporary numbers) — dynamic lookup only; if a hardcoded dependency is unavoidable (e.g. a branch with a slash), register it in Entry Criteria (`documentation/GitHubTestingStructure.md`) and keep it alive for the suite's lifetime

## Request/Response comparison

- After POST/PATCH, compare request fields against response to verify API returned what was sent
- Use `AssertHelper.ShouldMatchRequest<TRequest, TResponse>()` — not inline assertions
- Helper maps request properties to response by name (case-insensitive), skips nulls
- Validate server-generated fields separately: `id` (positive), `created_at` (recent), `state` (expected value)

## E2E cleanup

- **Create → remove, never retain** — every remote resource a test creates is deleted once the flow ends; leaving a test record behind is never the default ("better to create once more and delete than to keep a test entry"). When the API has no delete (issues → close, PRs → close, merge/commit history → immutable), use the closest available cleanup and document the residue — a deletable resource (branch, comment) is always actually deleted
- **All cleanup lives in `[OneTimeTearDown]` (last member of the class) via a fixture-registry pattern** — never per-test `finally`:
  1. fixture holds `private readonly List<T> _createdX = [];` (test data, allowed by the "only tests" rule)
  2. **register-then-assert**: add the created resource id to the registry IMMEDIATELY after the create call, before any assertion — a failing assert must not leak the resource
  3. teardown iterates the registry and calls the shared helpers (`CleanupCommentAsync`, `CleanupIssueAsync`, `CleanupGitRefAsync`, `CleanupPullRequestAsync` → single `RunCleanupAsync` core)
- **Tests stay pure asserts** — no cleanup code inside `[Test]` bodies; a re-delete of an already-deleted resource is safe (NotFound is tolerated)
- **Multi-step builders clean their own partials** — e.g. `CreateScratchPullRequestAsync` deletes the branch and rethrows if the pipeline fails midway (orphan blob/tree/commit objects are harmless)
- Cleanup order inside a teardown: comments before issues, branches before PRs, PRs before repos
- If cleanup fails, log warning but don't fail the run — resource can be manually cleaned; an unhandled cleanup exception would replace the original test failure and mask its cause
- Use sandbox repo for all write operations — never target production data

## Temp and artifact cleanup

Run artifacts accumulate without bound — every mechanism MUST be wired to the run lifecycle:

- **Temp logs** (`%TEMP%\GredjaTestRun`) — cleaned by `TestRunWorkspace.PrepareRun()`
  (run-marker based, once per run, two testhosts cannot double-run it).
- **Repo artifacts** (`allure-results/`, `TestResults/*.md`) — cleaned by
  `TestRunReportGenerator.PrepareRun()`: files older than `ArtifactRetentionDays` (7) are deleted.
- **A new artifact output directory MUST be registered in `CleanupAccumulatedArtifacts()`**
  in the same commit that introduces it — unbounded growth is not allowed.
- **Cleanup MUST NOT fail the run**: catch specific exceptions (`IOException`,
  `UnauthorizedAccessException`), log a warning, continue.
- **Condition is age-based (`LastWriteTime`)**, never "delete everything" — sequential
  testhosts (Api → E2E) may already have written current-run files.

## Assertion helpers

- Use `ShouldHaveStatusCode()` for HTTP status checks — not `.Should().Be(HttpStatusCode.OK)`
- Use `ShouldHaveValidContract()` for ContractCheck tests (JSON round-trip + attribute validation)
- Use `ShouldHaveValidFields()` for Regression tests (attribute validation only)
- Use `ShouldMatchRequest()` for request/response comparison — not manual field mapping
- All helpers live in `Core/Helpers/Assertions/AssertHelper.cs`

## Document sync

When adding or changing endpoints, keep these documents in sync:
- `documentation/{Service}ObservableBehaviour.md` — source of truth for test design (fields, types, negatives)
- `documentation/{Service}TestPlan.md` — coverage tracking (which endpoints have tests, status)
- `Rules/*.md` — shared rules for all services (assertions, patterns, cleanup)
- If Observable Behaviour changes → update Test Plan status; if Test Plan adds endpoints → update Observable Behaviour
- **One canonical source per guidance topic**: test-generation instructions live ONLY in
  `.mimocode/skills/*` — never create parallel prompt libraries (they silently drift;
  `Prompts/` was deleted for this reason)
- **Risk coverage** — every Top-3 risk from `{Service}TestPlan.md` / `{Service}TestingStructure.md` must map to ≥1 test case; an uncovered risk is a coverage gap (found this way: PSD2 SCA risk had no test case — see `documentation/Katas/01-test-cases.md`)

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

**Step 4: Enforce negative floor** — minimum 5 negatives per endpoint, counted as **active** tests (`[Ignore]`d tests don't count toward the floor)
- If fewer than 5: generate more, each exercising a different failure mode
- Failure modes: no auth, invalid token, wrong scope, missing required field, non-existent resource, invalid value, boundary value
- **Missing-field rule (mandatory, ALL services)**: every required request field gets its OWN negative test that omits exactly that field (`*_Missing{Field}_*`); a single empty-body test does NOT replace per-field tests
- **Mock-API exception**: some mock endpoints cannot produce 5 distinct active failures — list endpoints (query params and any id silently accepted) and GetById endpoints that return 200 for any input. Document the achieved count and stop; never pad with `[Ignore]`d tests just to hit the number
- **Ceiling rule**: when distinct useful failure modes are exhausted (verify with live probes first), stop even below 5 and note the achieved count — padding with redundant tests is worse than a documented gap. Useful beats numerous: a uniform GitHub auth middleware doesn't need a 401 test on every route

**Step 5: Save** — target ~15-20 unique test cases per endpoint

**Expected yield per endpoint:** ~8-14 tests (5-6 happy, 5-6 negative, 2-3 edge)
**Never:** less than 3 tests per endpoint, less than 5 active negatives per endpoint without a documented ceiling (see Step 4)

**Don't duplicate attribute checks:** `ShouldHaveValidFields()` covers `[RequiredField]`, `[PositiveId]`, `[ValueRange]`. Per-field assertions only for things attributes CAN'T cover: unique IDs, boundary values, idempotency, cross-field invariants, category counts.

## Guarantee Data

When a GET endpoint may return an empty list, don't silently skip the test. Guarantee data exists via `[OneTimeSetUp]`.

**Pattern:**
```csharp
private int? _createdId;

[OneTimeSetUp]
public async Task OneTimeSetup()
{
    var response = await Get<List<T>>(Endpoint);
    if (response.Data!.Count == 0)
    {
        var create = await Post<Request, Response>(Endpoint, TestData);
        _createdId = create.Data!.Id;
    }
}

[OneTimeTearDown]
public async Task OneTimeTearDown()
{
    if (_createdId.HasValue)
        await Delete($"{Endpoint}/{_createdId}");
}
```

**Rules:**
- POST only in `[OneTimeSetUp]`, never in `[Test]` methods
- Track created resource ID for cleanup
- `[OneTimeTearDown]` deletes created resources
- If POST is unavailable — use `[Ignore]` with explanation, not `Inconclusive`
- Never use `if (Data.Any())` to skip assertions — data must exist before assertion runs
- **Documented-empty exception** — when OB documents the collection as empty in the sandbox (topics, tags, releases), an item-contract check may be guarded with `if (Count > 0)`; the empty state itself MUST be asserted in a separate Smoke test (200 + `NotBeNull`/`OnlyContain`) — never a silent skip

## Read-after-write visibility (GET + POST)

Extends Guarantee Data: the created record must be **visible to GET**, not just exist. Catches non-persisting writes, caching, lost/duplicated inserts.

```csharp
private List<{Endpoint}Model> _baseline;
private {Endpoint}Model _created;

[OneTimeSetUp]
public async Task OneTimeSetup()
{
    _baseline = (await Get<List<{Endpoint}Model>>(Endpoint)).Data!;
    var create = await Post<{Endpoint}Request, {Endpoint}Model>(Endpoint, TestData);
    _created = create.Data!;
}

[OneTimeTearDown]
public async Task OneTimeTearDown()
{
    await Delete<object>($"{Endpoint}/{_created.Id}");
}
```

**Assertions (split into two tests — one check-type category each):**
- `*_ReturnsCreatedRecord` (`Smoke`) — second GET: `HaveCount(_baseline.Count + 1)` and set-difference of Ids (`current - baseline`) equals exactly `{ _created.Id }`
- `*_CreatedRecordMatchesRequest` (`Regression`) — `record.ShouldMatchRequest(TestData)` for ALL fields, not only Id (POST may return a valid Id while persisting fields wrong)

**Mandatory for every POST flow — record-add speed check:**
- `*_RecordVisibleWithinTimeLimit` (`Performance`) — time from POST completion until the record is visible to GET. Poll with `WaitHelper.WaitUntilAsync(action, condition, timeout: TimeSpan.FromMilliseconds(TestConfig.MaxResponseTimeMs))` (`Core/Helpers/Waiting/` — first attempt is immediate, so instant APIs finish on attempt 1 without polling overhead). Assert `result.IsSuccess`; include `result.Elapsed` / `result.Attempts` / `result.LastValue` in the failure message — that IS the speed metric and the diagnostics
- APIs that never persist the record (fake APIs, e.g. JSONPlaceholder) — keep the test but `[Ignore]` it with documented behavior (fake-API exception below)

**Rules:**
- Baseline GET + POST go in `[OneTimeSetUp]`; tests only read and assert; `[OneTimeTearDown]` deletes the created record — cleanup is mandatory
- **Cleanup = delete if the API allows it.** If the service has a delete endpoint — use it (`DELETE`); only if deletion is genuinely unavailable (e.g. GitHub issues: no REST delete, see OB §17) fall back to soft-close (`PATCH state=closed`) and document the fallback in the OB
- Cleanup failure must not fail the run — log warning and continue (transient errors, rate limits)
- Set-diff by Id is the primary invariant; exact `+1` only holds in a controlled sandbox — parallel runs on shared public APIs break counts
- If POST does not persist (fake APIs, e.g. JSONPlaceholder) — `[Ignore]` with explanation of documented behavior
- If this pattern always POSTs, count-based tests (`ExpectedCount`) must use a dynamic baseline, not a constant
- GitHub: Phase 1 is read-only — apply this pattern in Phase 2 E2E (skill `/e2e-test-gen`); issue cleanup = `PATCH state=closed` (REST has no issue-delete endpoint — `DELETE /issues/{n}` → 404, see OB §17)

## Waiting for conditions (WaitHelper)

For APIs where visibility/state change is not immediate (eventual consistency, background processing) — poll instead of a single immediate GET: `WaitHelper.WaitUntilAsync(action, condition, timeout?, interval?)` in `Core/Helpers/Waiting/` returns `WaitResult<T>` (`IsSuccess`, `Elapsed`, `Attempts`, `LastValue`).

- **Use it when:** the record must appear / state must change within a time limit — assert `result.IsSuccess` plus a bound on `result.Elapsed` (e.g. `Elapsed < sla` for "within 3s", `Elapsed >= minDelay` for "no earlier than 15s"); name such tests `*_AppearsWithinTimeLimit`, category `Performance`
- **Do not use it when:** the API answers instantly (fake APIs like JSONPlaceholder) — polling only slows the suite; and for latency of a SINGLE request keep the existing pattern: `Stopwatch` + `TestConfig.MaxResponseTimeMs` (`*_ResponseTimeIsAcceptable`)
- On failure include `Elapsed`/`Attempts`/`LastValue` in the assertion message — they are the diagnostics for flaky timing tests
- Timing boundary "exactly at N seconds" is unverifiable with polling granularity — leave a margin (check "not earlier" one interval before the boundary) and clarify the requirement if strictness matters

## Test data for write operations

Derived from kata 6.W.3 (PII-safe test data). Applies to E2E and any POST/PATCH payloads.

- **Fictional values only, never PII** — E2E writes into the public sandbox repo `Gredja/AiTest`; issue/PR bodies are publicly visible forever. Synthetic IDs, invented names, tokenized payments (see `documentation/Katas/02-test-data.json` for the pattern)
- **Obfuscate by replacement, never by dropping** — keep the field shape, replace the value; dropping a field breaks e2e coverage
- **Vary ≥2 dimensions across test payloads** — don't run every write test against one static body (country/language, order size, status, payment method). If one dataset is intentionally enough, document why in the test class
- **Vary payload values via `Core/Helpers/Data/DataGenerator`** — generate per-run values (`RandomString`, `RandomIntExclusive`) for varying write fields instead of static literals; semantic negative-test data (invalid ids, boundary values, wrong-type markers) stays as named constants — its value IS the specification
- **Complex datasets get a method note** — which tool/prompts generated them, which fields are obfuscated, which dimensions are exercised, what is intentionally missing (pattern: `documentation/Katas/02-data-method.md`)
