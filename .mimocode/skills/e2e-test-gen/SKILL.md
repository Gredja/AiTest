---
name: e2e-test-gen
description: Use when the user wants to generate E2E tests for write operations (POST/PATCH/DELETE) with setup and teardown. Trigger on mentions of "e2e test gen", "generate e2e tests", "/e2e-test-gen", or testing create/update/delete scenarios.
---

# E2E Test Generation

## Purpose

**Role:** you act as a senior AQA engineer in Gredja writing E2E scenario chains (create → verify → mutate → cleanup). Resources are fictional; every created resource is registered before assert and deleted in the last `[OneTimeTearDown]` via fixture registry — never retained. You read Observable Behaviour first and never guess status codes or field names.

Generates NUnit E2E test classes for write operations (POST, PATCH, DELETE) with mandatory setup/teardown for resource lifecycle management. Each test creates resources, tests behavior, and cleans up after itself.

---

## Variable Placeholders

| Placeholder | Description | Example value |
|---|---|---|
| `{{service_name}}` | API service (must match existing directory in `E2E/`) | GitHub |
| `{{endpoint_name}}` | Resource to test (e.g. issues, comments, pull requests) | issues |

---

## Output Format Instruction

Model must return: test files (.cs) in C# format with namespace, class, setup, teardown, test methods. Format — code without comments.

After generation — return ONE JSON object as the FIRST part of the reply, no surrounding prose before it:

```json
{
  "files": {
    "created": ["path/to/NewChainTests.cs"],
    "modified": ["path/to/Existing.cs"]
  },
  "tests": { "total": 0, "active": 0, "ignored": 0 },
  "cleanupVerified": true,
  "safetyCheck": {
    "format": "pass",
    "testRun": { "filter": "Category={Service}E2E", "passed": 0, "failed": 0 },
    "issues": []
  },
  "suggestedCommitMessage": "Add {Service} {endpoint} E2E chain tests"
}
```

**Contract validation — the reply is invalid unless ALL hold:**
1. JSON parses; every key above is present; `safetyCheck.format` is exactly `pass` or `fail`.
2. `tests.total = active + ignored`.
3. `cleanupVerified = true` — every chain registers resources before assert and cleans them in `[OneTimeTearDown]` (fixture-registry); `false` invalidates the reply.
4. `safetyCheck.format = "pass"` AND `failed = 0` — otherwise `suggestedCommitMessage` MUST be omitted.
5. `files.created` lists only paths that did NOT exist before this run; `suggestedCommitMessage` matches `action + object`.
6. **Automated check (rules 1–2, 4):** write the JSON to a temp file and run `powershell -File Scripts/validate-skill-report.ps1 <file>` — it must print `PASS` (exit 0). On `FAIL`: fix the JSON and re-run; a report that fails the validator must not be sent.

A human-readable summary MAY follow the JSON; the JSON is the deliverable, the prose is optional.

---

## Input

User provides:
1. **Service name** — `{{service_name}}` (must match existing directory in `E2E/`)
2. **Endpoint name** — `{{endpoint_name}}` (e.g. `issues`, `comments`, `pulls`)
3. **Sandbox repo** — target repo for write operations (e.g. `Gredja/AiTest`)

If not provided, ask which service and endpoint to test.

---

## File Convention

| What | Path pattern |
|---|---|
| Observable Behaviour | `documentation/{Service}ObservableBehaviour.md` |
| E2E Tests | `E2E/{Service}/Tests/{Endpoint}Tests.cs` |
| E2E Test Base | `E2E/{Service}/{Service}E2ETestBase.cs` |
| Endpoints config | `Core/Config/{Service}Endpoints.cs` |
| Request models | `Core/Models/{Service}/` |
| Namespace (tests) | `E2E.{Service}.Tests` |

---

## Flow

### Step 1: Read Observable Behaviour

Read `documentation/{Service}ObservableBehaviour.md`. Focus on:
- POST/PATCH/DELETE endpoint sections (Write Operations)
- Request Body table — required vs optional fields
- Response status codes (201 Created, 200 OK, 204 No Content)
- Negative cases (401, 403, 404, 422)
- Risk Framing → Write Operation Risks

### Step 2: Design resource lifecycle

For each E2E scenario, map the lifecycle:

```
Setup:    POST → create resource → store ID
Test:     GET/PATCH/DELETE → verify behavior
Teardown: DELETE → clean up resource → verify cleanup
```

**Cleanup order matters:**
1. Delete comments before issues
2. Delete branches before PRs
3. Delete PRs before repos (if applicable)

### Step 3: Plan the Test List (mandatory gate)

Before writing ANY code — build the plan and STOP for user approval. This step is mandatory for every endpoint; skipping it invalidates the run.

**Part 1 — Seed table.** Apply seed methodology from `Rules/test-practices.md`:

**Happy path seeds:**
- Create → verify response → verify via GET → delete → verify deleted
- Create with all fields → verify all fields present
- Update → verify changed fields → verify unchanged fields preserved
- Create multiple → verify count → delete one → verify count decremented

**Negative seeds:**
- Create without auth → 401
- Create with missing required field → 422
- Update non-existent resource → 404
- Delete non-existent resource → 404
- Create in non-existent repo → 404

**Edge seeds:**
- Create with special chars in fields
- Rapid create-delete-create cycle
- Concurrent creates

Output the seed table as Part 1 of the plan.

**Part 2 — Full list of future test methods.** Translate the seed table into the exact code-level plan — every future test method as it will appear in the file:
- method name (`{Endpoint}_{Case}_{Expected}`, PascalCase)
- `[Category(...)]` (Smoke / Regression / Negative / …)
- `[Description(...)]`
- target file (`E2E/{Service}/Tests/{Endpoint}Tests.cs`)

Also list files to be **created** and files to be **modified**.

**Gate:** output Part 1 + Part 2 in chat and STOP. Ask the user to approve or request modifications. On modifications → update the plan, re-present, wait again. Write code ONLY after explicit approval. Never jump straight from seeds to code.

### Step 4: Generate test class

**Mandatory rules reading (before writing any code):** open and read the full list of project rules — do not rely on memory or on digests duplicated in AGENTS.md:
- `Rules/code.md`, `Rules/code-style.md`, `Rules/code-principles.md`
- `Rules/models.md`, `Rules/assertions.md`
- `Rules/test-practices.md`, `Rules/categories.md`
- `Rules/comments.md`, `Rules/config.md`
- `Rules/workflow.md`, `Rules/git.md`

`Rules/ui-testing.md` and `Rules/localization-testing.md` apply only when the chain belongs to a UI/localization scope — otherwise skip them. If any rule here contradicts this skill, the Rules file wins — flag the contradiction to the user instead of silently choosing one.

Generated tests must match the approved plan from Step 3 — no extra tests, no dropped tests.

**Required pattern** (fixture-registry; cleanup ONLY in `[OneTimeTearDown]` LAST member — no per-test `[SetUp]`/`[TearDown]`):

```csharp
using AllureAdapter;
using Core.Config;
using FluentAssertions;
using System.Net;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.{Service}.Tests;

[TestFixture]
[AllureNUnit]
[Category("{Service}E2E")]
public class {Endpoint}Tests : {Service}E2ETestBase
{
    private readonly List<int> _createdIds = [];

    [Test]
    [Category("Smoke")]
    [Description("E2E-1 Create returns 201")]
    public async Task Create_ReturnsCreated()
    {
        var response = await Post<CreateRequestModel, ResponseModel>(
            {Service}Endpoints.{Endpoint}, _testBody, ...);
        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIds.Add(response.Data!.Id);   // register-then-assert

        var readBack = await Get<ResponseModel>(
            {Service}Endpoints.{Endpoint}ById, IdParam(response.Data.Id));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data!.Id.Should().Be(response.Data.Id);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // fixture-registry cleanup — warning-only, never fail the run;
        // cleanup helpers live in the base class / helpers, NOT here
    }
}
```

**Key rules:**
- Cleanup ONLY in `[OneTimeTearDown]` (LAST class member) via fixture-registry — register IDs immediately after creation, BEFORE asserts (register-then-assert)
- No per-test `[SetUp]`/`[TearDown]` cleanup (Rules/test-practices: teardown-last)
- No helper methods inside `*Tests.cs` — cleanup chains go to the base class / helpers
- Create inside the test (or `[OneTimeSetUp]` with status assert); setup failure must fail loudly, not skip
- Use `ShouldMatchRequest()` for request/response comparison
- **Test data:** fictional values only (public sandbox repo), vary ≥2 dimensions — see `Rules/test-practices.md` → "Test data for write operations"

**Negative example — a chain that must NEVER be generated (and why):**
```csharp
[Test]
public void TestCleanup()
{
    var created = Post<CreateIssueModelRequest, IssueModelResponse>(
        GitHubEndpoints.RepoIssues, new CreateIssueModelRequest { Title = "test" }, TestRepoParam());
    try
    {
        Assert.That(created.Data!.State, Is.EqualTo("open"));
    }
    finally
    {
        Delete<object>(GitHubEndpoints.RepoIssueById, IssueParams(created.Data!.Number));
    }
}
```
Why it is rejected: cleanup in per-test `finally` instead of `[OneTimeTearDown]` fixture-registry (teardown-last rule); no register-before-assert (a failed assert path leaks if `finally` itself throws); helper-style orchestration inside the test class; NUnit `Assert.*` (FluentAssertions only); no `[Category]`/`[Description]`; static "test" data (must be fictional + varied via `DataGenerator`).

### Step 5: Cleanup helpers live in base, not in tests

Complex cleanup chains (delete comments → then issue) belong in `{Service}E2ETestBase` (or a dedicated helper) — never as private methods in `*Tests.cs` (Rules/test-practices: no helpers in test classes).

### Step 6: Follow All Rules

- **Code:** PascalCase, file-scoped namespaces, async, no magic numbers — see `Rules/code.md`
- **Cleanup:** mandatory, fixture-registry + `[OneTimeTearDown]` last — see `Rules/test-practices.md`
- **Assertions:** ShouldHaveStatusCode, ShouldHaveValidFields, ShouldMatchRequest (reduces code vs manual field comparison) — see `Rules/assertions.md`
- **Non-existent IDs:** dynamic (GET all → maxId + 1; concurrent runs → maxId + Offset) — see `Rules/test-practices.md`
- **Mock API behavior:** `[Ignore]` with explanation for known mock limitations — see `Rules/test-practices.md`
- **Seed methodology:** 5 seeds → expand to table → enforce 5+ active negatives → one negative test per required request field (`*_Missing{Field}_*`, mandatory) — see `Rules/test-practices.md`

### Step 7: Safety Check

**Plan-vs-implementation reconciliation (mandatory, first):** compare the generated code against the plan approved in Step 3. Do it mechanically — Part 2 of the plan is the source of truth: extract every test method name from the generated file(s) (Grep `public async Task` in the target file), diff the extracted list against Part 2's method names. The table below is the OUTPUT of that diff, never a hand-typed list (hand-typing drifts on sets of 20+). Every planned test method must exist; no unplanned tests may appear.

| # | Planned test (method name) | Category | In code | Status |
|---|---|---|---|---|
| 1 | `CreateIssue_ReturnsCreated` | Smoke | ✅ | matched |

Mismatches (renamed / missing / extra method) → fix the code to match the plan (or, if the change was requested by the user during generation, update the plan first) → re-run the reconciliation until every row is `matched`. Proceed below only when the table is fully green.

Then:
1. `dotnet format --verify-no-changes` — fix if needed
2. `dotnet build` — verify compilation
3. `dotnet test --filter "Category={Service}E2E"` — verify tests pass

**Test run report (mandatory, after the run):** build a per-test table from the run results:

| # | Test method | Category | Result | Duration | Error (if failed) |
|---|---|---|---|---|---|
| 1 | `CreateIssue_ReturnsCreated` | Smoke | ✅ passed | 1.2s | — |
| 2 | `CreateIssue_MissingTitle_Returns422` | Negative | ❌ failed | 0.8s | expected 422, actual 200 |

0 failed → continue to Step 8. ≥1 failed → fix and re-run (or `[Ignore]` with explanation for a documented mock limitation) — the run must end green before the report step. `safetyCheck.testRun` in the JSON contract is filled from this table.

### Step 8: Report

Return the JSON contract from "Output Format Instruction" (`files` / `tests` / `cleanupVerified` / `safetyCheck` / `suggestedCommitMessage`) after validating rules 1–6 (rule 6 = the validator script must print `PASS`). The plan-vs-implementation reconciliation table and the test run report table from Step 7 are **required** parts of the reply — include them alongside the JSON (before or after, not omitted). A human-readable summary may follow — the JSON + both tables are the deliverable, the prose is optional.

---

## Key Differences from api-test-gen

| Aspect | api-test-gen (read + single writes) | e2e-test-gen (scenario chains) |
|---|---|---|
| HTTP methods | GET + single-endpoint write tests | multi-step POST/PATCH/DELETE scenarios |
| Setup/Teardown | Guarantee Data / OneTimeSetUp for write singles | fixture-registry + OneTimeTearDown last |
| Resource cleanup | Yes (write singles) | Yes — multi-step create→verify→mutate→cleanup |
| Test independence | Stateless reads | Each chain creates own resources |
| Sandbox repo | Reads from any repo | Writes only to sandbox |
| Risk level | Low–medium | High (creates/deletes resources) |
| Mock behavior | Rarely ignored | Often ignored (mock APIs don't persist) |

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-02-21 | Initial commit | Alexey |
| 1.1 | 2026-09-30 | Fixed stale `Get<T>` signature in Setup example — removed `Method.Get` argument | MiMo |
| 1.2 | 2026-09-30 | Added test data rule reference (fictional values, vary dimensions) from kata 6.W.3 | MiMo |
| 1.3 | 2026-10-02 | Seed methodology: active negatives only + one negative per required request field | MiMo |
| 1.4 | 2026-11-04 | Machine-checkable JSON report (incl. cleanupVerified), mandatory plan gate (seed table → approval → code), negative example (rejected chain with violation list) | MiMo |
| 1.5 | 2026-11-05 | Plan gate → standalone "Plan the Test List" (Part 1 seed table + Part 2 full test-method list); Step 4: mandatory Rules/*.md reading + match-approved-plan; Step 7: reconciliation + per-test run report tables (green before Step 8); Step 8: both tables required alongside JSON | MiMo |
| 1.6 | 2026-11-05 | Contract rule 6: automated validation of the JSON report via Scripts/validate-skill-report.ps1 (must print PASS before the reply is sent) | MiMo |
| 1.7 | 2026-11-05 | Step 7: reconciliation made mechanical — Grep method names vs approved Part 2, table is the diff output (never hand-typed) | MiMo |
