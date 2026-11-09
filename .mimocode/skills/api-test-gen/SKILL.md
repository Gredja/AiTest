---
name: api-test-gen
description: Use when the user wants to generate API tests for any service endpoint. Trigger on mentions of "api test gen", "generate API tests", "/api-test-gen", or testing a new endpoint.
---

# API Test Generation

## Purpose

**Role:** you act as a senior AQA engineer working in the Gredja test-automation project. You generate NUnit API tests strictly from Observable Behaviour and existing code — not from memory. You follow `Rules/test-practices` (seed methodology, negative floor), `Rules/assertions` (FluentAssertions only), and `Rules/code` (naming, file layout). You never invent endpoints, fields, or error messages: every expectation must trace to a live probe or an OB section.

Generates a complete set of NUnit API tests for any REST API service endpoint — for AQA Engineer at the test automation stage.

---

## Variable Placeholders

| Placeholder | Description | Example value |
|---|---|---|
| `{{service_name}}` | API service (matches directory/class names) | JsonPlaceholder, GitHub, Swapi |
| `{{endpoint_name}}` | Endpoint to test | users, posts, todos |

---

## Output Format Instruction

Read `references/report-contract.md` **in full** and follow it exactly: the JSON report
shape (`files` / `tests` / `safetyCheck` / `suggestedCommitMessage`), contract validation
rules 1–6 (rule 6 = the validator script must print `PASS`), and the reply layout
(JSON first, prose optional).

---

## Input

User provides:
1. **Service name** — `{{service_name}}` (must match existing directory name in `Api/`)
2. **Endpoint name** — `{{endpoint_name}}` (e.g. `products`, `users`, `posts`)

If not provided, ask which service and endpoint to test.

## File Convention (service-agnostic)

All paths use `{Service}` as the service name (PascalCase, matches directory names):

| What | Path pattern |
|---|---|
| Observable Behaviour | `documentation/{Service}ObservableBehaviour.md` |
| Observable Behaviour Template | `documentation/ObservableBehaviourTemplate.md` |
| Endpoints config | `Core/Config/{Service}Endpoints.cs` |
| Models | `Core/Models/{Service}/` |
| Tests | `Api/{Service}/Tests/{Category}/{Endpoint}Tests.cs` |
| Param helper | JsonPlaceholder: `Api/JsonPlaceholder/Helpers/JsonPlaceholderParamHelper.cs`; GitHub: `Core/Helpers/GitHub/GitHubParamHelper.cs` |
| Namespace (tests) | `Api.{Service}.{Category}` (e.g. `Api.GitHub.Issues`, `Api.JsonPlaceholder.Posts`) |
| Namespace (models) | `Core.Models.{Service}` |
| Category (class) | `[Category("{Service}")]` |

Before generating — verify that `Core/Config/{Service}Endpoints.cs` exists. If not, create it.

## Flow

### Step 1: Research the endpoint

0. **Read the existing code FIRST** — Glob/Grep for the endpoint's existing test class, models, and constants. Extend what exists; NEVER create duplicate files (both models failed this in `documentation/Katas/model-selection-note.md` — they generated new files instead of working with existing ones because they didn't read the project code first)
1. **Check Observable Behaviour** — if `documentation/{Service}ObservableBehaviour.md` exists, read it first. It contains: expected status codes, response types, field definitions with types, pagination, filtering, and negative cases. Use it as the primary source for test design.
2. **Create Observable Behaviour if missing** — if `documentation/{Service}ObservableBehaviour.md` does not exist, create it from `documentation/ObservableBehaviourTemplate.md`. Fill in: endpoint sections, field definitions, negative cases, validation rules, risk framing. This document is the source of truth for test generation.
3. Fetch the endpoint from the API (use BaseUrl from `testsettings.json` → `TestConfig.{Service}BaseUrl`)
4. Analyze the response structure — what fields, what types, what's nullable
5. Check for sub-endpoints: `/{endpoint}/{id}`, nested resources
6. Count total items (needed for `ExpectedCount`)

### Step 2: Add constants to Endpoints file

Update `Core/Config/{Service}Endpoints.cs`:

Add:
- `Expected{Endpoint}Count` — total items from API
- `Test{Endpoint}Id` — valid ID for single-item tests (use ID = 1)
- `{Endpoint}` — path string (e.g. `/users`)
- `{Endpoint}ById` — path with segment (e.g. `/users/{id}`)

### Step 3: Generate Models

Create or update models in `Core/Models/{Service}/`:

**Main model** (`{Endpoint}Model.cs`):
- Suffix: `ModelResponse` for response, `ModelRequest` for request body (nested/supporting models without suffix)
- Always includes `Id` field with `[PositiveId]` attribute
- Reference types (string, object, nested model): `[RequiredField]`, no `?`, no initializer
- Value types (int, decimal, double): `?` only if JSON field can be null/absent
- `[ValueRange(min, max)]` on numeric fields with known bounds (e.g. rating 0-5, price >= 0)
- Namespace: `Core.Models.{Service}`
- Pure data container — no constructors, no validation logic

**Attribute style** — always on separate line above property:
```csharp
[PositiveId]
public int Id { get; set; }

[RequiredField]
public string Title { get; set; }

[ValueRange(0, 5)]
public double Rate { get; set; }
```

**Nested models** — separate file for each nested object (e.g. `RatingModel`, `UserNameModel`, `AddressModel`).

**Check for shared base classes** — if 2+ models share identical fields with same types, create a base class in `Core/Models/Generic/` and inherit (e.g. `UserOwnedModel` for entities with `Id` + `UserId` + `Title`).

If models already exist — verify they match current API response.

### Step 4: Plan the Test List (mandatory gate)

Read `references/plan-gate.md` **in full** and follow it exactly: Part 1 (seed table,
negative floor, one `*_Missing{Field}_*` per required body field) + Part 2 (full list of
future test methods with names, categories, descriptions, target file, files
created/modified) → output both parts and STOP. Code ONLY after explicit user approval.

### Step 5: Generate Tests

**Mandatory rules reading (before writing any code):** open and read the full list of project rules — do not rely on memory or on digests duplicated in AGENTS.md:
- `Rules/code.md`, `Rules/code-style.md`, `Rules/code-principles.md`
- `Rules/models.md`, `Rules/assertions.md`
- `Rules/test-practices.md`, `Rules/categories.md`
- `Rules/comments.md`, `Rules/config.md`
- `Rules/workflow.md`, `Rules/git.md`

`Rules/ui-testing.md` and `Rules/localization-testing.md` apply only when the endpoint belongs to a UI/localization scope — otherwise skip them. If any rule here contradicts this skill, the Rules file wins — flag the contradiction to the user instead of silently choosing one.

**Hard constraints (every line generated in this step):**
- FluentAssertions only — never NUnit `Assert.*`; status/errors via `ShouldHaveStatusCode`/`ShouldHaveError` (see `Rules/assertions.md`)
- Typed response models only — never `JsonElement` + `TryGetProperty`
- Async only — `await` through the shared `Get/Post/...` wrappers; never the sync `Execute`
- No comments in generated code (regex / non-obvious WHY only — `Rules/comments.md`)

Generated tests must match the approved plan from Step 4 — no extra tests, no dropped tests.

**Base class** — check if `{Service}RequestHelper` exists in `Core/Helpers/`. If not, create it extending `RequestHelper`. If it exists, use it.

**Param helpers** — use shared helpers for URL segments and query parameters:
- JsonPlaceholder: `using static Api.JsonPlaceholder.Helpers.JsonPlaceholderParamHelper;`
- GitHub: `using static Core.Helpers.GitHub.GitHubParamHelper;`
- If helper doesn't exist, create it with `ProductIdParam`/`UserIdParam`/`PostIdParam` as needed

For custom params, use `ParamType` enum (from `Core.Models`) — never magic strings:
```csharp
new() { Type = ParamType.UrlSegment, Key = "id", Value = id }
new() { Type = ParamType.Parameter, Key = "userId", Value = userId }
```

**Request body** — extract repeated bodies to `static readonly` fields:
```csharp
private static readonly PostModelRequest TestPost = new() { UserId = 1, Title = "Test Post", Body = "Test Body" };
```

**Pattern:** Use `ShouldHaveValidContract()` for ContractCheck (JSON round-trip + attribute validation). Use `ShouldHaveValidFields()` for Regression (attributes only). Use `ShouldMatchRequest()` for request/response comparison — replaces manual field-by-field assertions.

**Positive template:** the complete `GetAll{Endpoint}` class template lives in `references/test-template.md` — read it and follow its structure (usings, fixture attributes, category/description pairs, the seven base tests).

**Negative example — a test that must NEVER be generated (and why):**
```csharp
[Test]
public void Test1()
{
    // checks that deletion works
    Thread.Sleep(2000);
    var response = Client.Execute(new RestRequest("/releases/999", Method.Delete));
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    Assert.That(response.Content.Contains("wrong release id"));
}
```
Why it is rejected: name `Test1` says nothing (needs `Endpoint_Case_Expected`); `Thread.Sleep` instead of real waits; sync `Execute` (async only); hardcoded `/releases/999` (endpoint from config; ids dynamic — only 0/-1 are safe static); NUnit `Assert.*` (FluentAssertions only); hand-rolled `.Contains` on a free string instead of `ShouldHaveError` with the OB-documented message; invented error text (must come from OB/`GitHubErrors`); excess comment; no `[Category]`/`[Description]`.

**For negative tests (non-existent ID):**
- Use dynamic approach: `GET all → maxId + 1` (concurrent writes in same run → `maxId + Offset`, see Rules/test-practices.md)
- Never hardcode 999 or any static number
- If API returns 200 instead of 404 — use `[Ignore]` with explanation

**Optional Setup/Teardown for efficiency:**
- Use `[OneTimeSetUp]` to fetch shared data once (e.g. `maxId` for dynamic negatives, item list for field validation)
- **Guarantee Data:** if GET returns empty → POST create → track ID for cleanup
- Store in `private` fields — reuse across tests in the class
- `[OneTimeTearDown]` deletes created resources
- Example:
```csharp
private List<{Endpoint}Model> _allItems;
private int _maxId;
private int? _createdId;

[OneTimeSetUp]
public async Task OneTimeSetup()
{
    var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
    _allItems = response.Data!;
    _maxId = _allItems.Max(x => x.Id);

    if (_allItems.Count == 0)
    {
        var create = await Post<{Endpoint}Request, {Endpoint}Model>(
            {Service}Endpoints.{Endpoint}, TestData);
        _createdId = create.Data!.Id;
        _allItems = (await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint})).Data!;
    }
}

[OneTimeTearDown]
public async Task OneTimeTearDown()
{
    if (_createdId.HasValue)
        await Delete<object>($"{Service}Endpoints.{Endpoint}/{_createdId}");
}
```
- Do NOT use `[SetUp]`/`[TearDown]` for read-only tests — each test should be independent
- Write flows: baseline GET + POST in `[OneTimeSetUp]`, cleanup in `[OneTimeTearDown]` (last member, fixture-registry) — see `Rules/test-practices.md`

**Read-after-write visibility (when the endpoint's service supports POST):**
- Extend the setup into an assertion: baseline GET → POST in `[OneTimeSetUp]` → tests assert the collection grew by exactly 1, set-diff by Id equals the created Id, and `ShouldMatchRequest()` compares the POST request against the new record in the GET response (all fields, not only Id) → `[OneTimeTearDown]` deletes the record (cleanup failure = warning, not test failure)
- Full pattern: `Rules/test-practices.md` → "Read-after-write visibility (GET + POST)"

### Step 6: Follow All Rules

- **Code:** PascalCase, file-scoped namespaces, async (`ExecuteAsync`), no magic numbers or strings — see `Rules/code.md`
- **Code style:** error handling, LINQ, strings, null safety — see `Rules/code-style.md`
- **Categories:** `[Category("{Service}")]` on class; test category on every method — see `Rules/categories.md`
- **Non-existent IDs:** Dynamic only — GET all → `maxId + 1` (concurrent runs → `maxId + Offset`) — see `Rules/test-practices.md`
- **Assertions:** FluentAssertions only — see `Rules/assertions.md`
- **JSON parsing:** Always parse responses into typed models — never use `JsonElement` + `TryGetProperty` in tests. Create model for every nested JSON structure — see `Rules/assertions.md`
- **Models:** suffix Model/Request, attributes on separate lines, pure data containers — see `Rules/models.md`
- **Config:** Use `{Service}Endpoints.*` from Endpoints file — see `Rules/config.md`
- **Comments:** No comments unless regex or non-obvious WHY — see `Rules/comments.md`
- **Negative tests:** `[Ignore]` with explanation for API known bugs — see `Rules/test-practices.md`
- **Access modifiers:** narrowest possible — private > protected > public — see `Rules/code.md`
- **Request/Response comparison:** Use `ShouldMatchRequest()` for POST/PATCH — see `Rules/assertions.md`
- **Write payloads:** vary fields per run via `Core/Generators/DataGenerator` (`RandomString`, `RandomIntExclusive`); semantic negative-test data stays as named constants — see `Rules/test-practices.md` → "Test data for write operations"

### Step 7: Safety Check

**Plan-vs-implementation reconciliation (mandatory, first):** compare the generated code against the plan approved in Step 4. Do it mechanically — Part 2 of the plan is the source of truth: extract every test method name from the generated file(s) (Grep `public async Task` in the target file), diff the extracted list against Part 2's method names. The table below is the OUTPUT of that diff, never a hand-typed list (hand-typing drifts on sets of 20+). Every planned test method must exist; no unplanned tests may appear.

| # | Planned test (method name) | Category | In code | Status |
|---|---|---|---|---|
| 1 | `GetAllUsers_ReturnsOk` | HealthCheck | ✅ | matched |

Mismatches (renamed / missing / extra method) → fix the code to match the plan (or, if the change was requested by the user during generation, update the plan first) → re-run the reconciliation until every row is `matched`. Proceed below only when the table is fully green.

Then:
1. Run `dotnet format --verify-no-changes` — fix if needed
2. Run `dotnet test --filter "GetAll{Endpoint}"` — verify new tests pass

**Test run report (mandatory, after the run):** build a per-test table from the run results:

| # | Test method | Category | Result | Duration | Error (if failed) |
|---|---|---|---|---|---|
| 1 | `GetAllUsers_ReturnsOk` | HealthCheck | ✅ passed | 0.3s | — |
| 2 | `GetAllUsers_InvalidId_ReturnsNotFound` | Smoke | ❌ failed | 0.5s | expected 404, actual 200 |

0 failed → continue to Step 8. ≥1 failed → fix and re-run (or `[Ignore]` with explanation for a documented API bug) — the run must end green before the report step. `safetyCheck.testRun` in the JSON contract is filled from this table.

### Step 8: Report

Return the JSON contract from "Output Format Instruction" (`files` / `tests` / `safetyCheck` / `suggestedCommitMessage`) after validating rules 1–6 (rule 6 = the validator script must print `PASS`). The plan-vs-implementation reconciliation table and the test run report table from Step 7 are **required** parts of the reply — include them alongside the JSON (before or after, not omitted). A human-readable summary may follow — the JSON + both tables are the deliverable, the prose is optional.

---

**Skill provenance** (original author test-run and peer-review record) lives in `references/history.md`.

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-06-25 | Initial commit | Alexey |
| 1.1 | 2026-06-25 | Added Purpose, Variable Placeholders table, Output Format Instruction, Peer Review | Alexey |
| 1.2 | 2026-06-25 | Made service-agnostic: removed hardcoded FakeStore/JsonPlaceholder, added File Convention table, dynamic base class discovery | Alexey |
| 1.3 | 2026-02-21 | Added Observable Behaviour document check in Step 1 — read `documentation/{Service}ObservableBehaviour.md` as primary source for test design | Alexey |
| 1.4 | 2026-02-21 | Added Observable Behaviour template reference, auto-create from template if missing, document sync rule in test-practices.md | Alexey |
| 1.5 | 2026-09-30 | Added Read-after-write visibility reference in Step 4 (GET+POST pattern) | MiMo |
| 1.6 | 2026-09-30 | Fixed stale `Get<T>` signature in Step 4 template — removed `Method.Get` argument | MiMo |
| 1.7 | 2026-09-30 | Step 1: added "read existing code first — never duplicate files" (from model-selection-note katka) | MiMo |
| 1.8 | 2026-10-02 | Step 4: negative floor counts active tests only, mock-API list exception, one negative per required request field | MiMo |
| 1.9 | 2026-10-02 | Step 5: write payloads vary via Core/Helpers/DataGenerator | MiMo |
| 1.10 | 2026-11-04 | Output report → machine-checkable JSON contract: required keys, counter sum invariant, gate-linked commit suggestion, created/modified split | MiMo |
| 1.11 | 2026-11-04 | Step 4: hard style constraints at the point of action (FluentAssertions / typed models / async-only / no comments) — was final-checklist-only in Step 5 | MiMo |
| 1.12 | 2026-11-04 | Step 4: mandatory plan gate (seed table → user approval → code) + negative example (rejected test with violation list) | MiMo |
| 1.13 | 2026-11-05 | Step 4 split: standalone mandatory "Plan the Test List" gate (seed table + full test-method list with names/categories/files → user approval) before code generation; old Step 4 → Step 5, steps 5-7 renumbered to 6-8 | MiMo |
| 1.14 | 2026-11-05 | Step 7: mandatory plan-vs-implementation reconciliation table (planned vs generated, green before format/test) ; Step 8: reconciliation table is a required part of the report | MiMo |
| 1.15 | 2026-11-05 | Step 7: mandatory per-test run report table (method/result/duration/error), green before Step 8; Step 8: run table required alongside JSON | MiMo |
| 1.16 | 2026-11-05 | Step 5: mandatory reading of the full Rules/*.md set before code generation (Rules file wins on conflict; ui-testing/localization-testing only for their scopes) | MiMo |
| 1.17 | 2026-11-05 | Contract rule 6: automated validation of the JSON report via Scripts/validate-skill-report.ps1 (must print PASS before the reply is sent) | MiMo |
| 1.18 | 2026-11-05 | Step 7: reconciliation made mechanical — Grep method names vs approved Part 2, table is the diff output (never hand-typed) | MiMo |
| 1.19 | 2026-11-08 | P4 anti-dilution split: Step 4 gate, Output Format contract, GetAll template, and author history moved to references/ (plan-gate, report-contract, test-template, history) — main file shortened with read-in-full pointers; behavior unchanged | MiMo |
