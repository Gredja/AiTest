---
name: ui-test-gen
description: Use when the user wants to generate UI tests for a web interface with Playwright (.NET + NUnit) in the Gredja Ui project. Trigger on mentions of "ui test gen", "generate UI tests", "/ui-test-gen", "сгенерируй UI тесты", testing a new page, screen or browser flow (navigation, create via UI, issue forms). Not for API tests (use api-test-gen) and not for API scenario chains (use e2e-test-gen).
---

# UI Test Generation (Playwright)

## Purpose

**Role:** you act as a senior AQA engineer in Gredja writing Playwright UI tests. Locators come only from a live `locator-dump` snapshot — never from memory. Feature tests load `storageState` and never log in; UI writes are cleaned up through the API. Hard rules below are non-negotiable.

Generates page-objects and NUnit UI tests for the Gredja `Ui/` project (Microsoft.Playwright + Chromium) — for AQA Engineer at the test automation stage. Design follows the XRM_Autotest reference: page-objects expose business actions, tests orchestrate and verify.

---

## Important (hard rules — read before anything else)

1. **Locators are role/text/label-based only** — `GetByRole`, `GetByText`, `GetByLabel`, `GetByPlaceholder`. NEVER CSS-classes or XPath on the page under test (GitHub generates hashed BEM-classes that change between releases). Every locator must be verified against a **live snapshot** of the page before it is written (see Step 1).
2. **No login/logout in feature tests** — the fixture loads `storageState` from `.auth/github-ui-state.json` (gitignored). A logout breaks every parallel colleague. Login is only the dedicated Smoke test `LoginSignInTests` (anonymous context, saves state after assert).
3. **UI write → API cleanup** — resources created through the UI are removed in `[OneTimeTearDown]` (LAST member of the class) via Core API helpers by composition (`GitHubUiTestBase` does not inherit `GitHubTestBase`): issue → close via PATCH (warning-only, never fail the run). Register in the registry immediately after creation, BEFORE any assert (register-then-assert).
4. **No `WaitForTimeout`** without a written WHY-comment — Playwright auto-waits on actionability; poll only for documented eventual-consistency with `WaitHelper`.
5. **Test classes contain only tests** — actions live in page-objects, shared browser logic in the fixture; no private helper methods in `*Tests.cs`.
6. **Never duplicate** — Glob/Grep existing page-objects and test classes first; extend, never create a second file for the same page.

---

## Variable Placeholders

| Placeholder | Description | Example |
|---|---|---|
| `{{service_name}}` | Web service under UI test (matches docs/namespaces) | GitHub |
| `{{area_name}}` | Page/area group (directory + class prefix) | Issues, Repository, Pulls |
| `{{flow_name}}` | What the test does | Create, Lifecycle, Navigation |

---

## Output Format Instruction

Model must return: page-object files (.cs) and test files (.cs) in C# with namespaces, classes, methods. Code without comments (comments only for non-obvious WHY).

After generation — return ONE JSON object as the FIRST part of the reply, no surrounding prose before it:

```json
{
  "files": {
    "created": ["path/to/Page.cs", "path/to/Tests.cs"],
    "modified": ["path/to/Existing.cs"]
  },
  "tests": { "total": 0, "byCategory": { "Smoke": 0, "Regression": 0, "Negative": 0 } },
  "locators": { "verifiedLive": 0, "draft": 0 },
  "safetyCheck": {
    "format": "pass",
    "testRun": { "filter": "Category=GitHubUi&FullyQualifiedName~...", "passed": 0, "failed": 0 },
    "issues": []
  },
  "suggestedCommitMessage": "Add {area} {flow} UI tests"
}
```

**Contract validation — the reply is invalid unless ALL hold:**
1. JSON parses; every key above is present; `safetyCheck.format` is exactly `pass` or `fail`.
2. `tests.byCategory` sums to `tests.total`.
3. `locators.draft = 0` — every locator in generated code came from a live `locator-dump` snapshot; a draft locator must not ship (fix or drop the test).
4. `safetyCheck.format = "pass"` AND `failed = 0` — otherwise `suggestedCommitMessage` MUST be omitted.
5. `files.created` lists only paths that did NOT exist before this run; `suggestedCommitMessage` matches `action + object`.

A human-readable summary MAY follow the JSON; the JSON is the deliverable, the prose is optional.

---

## Input

User provides:
1. **Service** — `{{service_name}}` (default `GitHub` — must have `Ui/` project)
2. **Area / page** — `{{area_name}}` (e.g. `issues`, `repo landing`)
3. **Flow to cover** — what to do and verify on that page (e.g. "create issue via UI, verify it appears in the list")

If any is missing — ask before generating.

---

## Preconditions

- `Ui/` project exists in `Gredja.slnx`. If not — stop, tell the user the scaffold phase must run first.
- `testsettings.json` has `Ui` section; `.auth/` is gitignored; `Scripts/install-playwright.ps1` was executed (chromium installed).
- Rules in context: `Rules/ui-testing.md`, `Rules/categories.md`, `Rules/test-practices.md` (teardown-last, register-then-assert, create→remove), `Rules/code.md`.

## File Convention

| What | Path pattern |
|---|---|
| UI Observable Behaviour | `documentation/{{service_name}}UiObservableBehaviour.md` |
| Base fixture (browser/context/page) | `Ui/github/Tests/Support/{{service_name}}UiTestBase.cs` |
| Browser wrapper (pages as properties) | `Ui/github/Tests/Support/BrowserPlaywright.cs` |
| Page-objects | `Ui/github/Pages/GitHubStream/{{area_name}}/{{area_name}}Page.cs` |
| Shared pages (header, nav) | `Ui/github/Pages/Common/` |
| Tests | `Ui/github/Tests/GitHubStream/{{area_name}}/{{area_name}}{{flow_name}}Tests.cs` |
| Test data files | `Ui/Data/` |
| Namespace (tests) | `Ui.Github.Tests.GitHubStream.{{area_name}}` |
| Namespace (page-objects) | `Ui.Github.Pages.GitHubStream.{{area_name}}` |
| Service category (class) | `[Category("GitHubUi")]` |
| Description reference | UI-OB section number, e.g. `"UI-1 Ordinary login…"` |

Before generating — verify the target directories exist; create missing page-object folder only after checking no sibling covers the same page.

---

## Flow

### Step 1: Research the page (live)

0. **Read existing code FIRST** — Glob `Ui/**` for the area's page-object and tests; extend what exists, never duplicate files.
1. **Read UI Observable Behaviour** — `documentation/{{service_name}}UiObservableBehaviour.md`. It is the source of truth: per section — verified locators (with `verified {date}` markers), element states, URL patterns, gotchas. If missing — create it while working: one section per page, locators only after live verification.
2. **Take a live snapshot of the page** — HARD RULE: only via `node Scripts/locator-dump.mjs <url> [--waitfor "text"]` (one-shot CDP dump of role+accessible name; the `playwright` skill / playwright-cli is forbidden for locator extraction). Aim it at the real page (`Ui.BaseUrl` from `testsettings.json`). Extract roles/texts/labels for every locator you are about to write. Do NOT invent selectors from memory of the HTML.
3. Note URL patterns, navigation path, and what eventual-consistency the page exhibits (list refresh after write?).

### Step 2: Design the test (mandatory plan gate)

- Map the flow to one test method (chain = one test, E2E-style) or several single-check tests.
- Pick ONE check-type category per test per `Rules/categories.md` boundaries: Smoke = single-response sanity (content, count); Regression = rule/invariant/state-transition; Negative = error/404/empty states.
- List UI-created resources → plan their API cleanup (issue → close via PATCH; comment → API DELETE — Core `RequestHelper` by composition).
- Prefer hybrid verification: **act via UI, assert via API** (Core `RequestHelper`) for created resources — the API read-back is the reliable oracle; UI asserts are for what only exists visually (badges, URLs, empty states).

**Part 1 — Plan table:** `# | Case | Category | Priority | Created resources → cleanup | Locator source (snapshot date)`.

**Part 2 — Full list of future test methods.** Translate the plan table into the exact code-level plan — every future test method as it will appear in the file:
- method name (`{{area}}_{{Flow}}_{{Outcome}}`, PascalCase)
- `[Category(...)]` (Smoke / Regression / Negative / …)
- `[Description(...)]` (UI-OB section reference)
- target file (`Ui/github/Tests/GitHubStream/{{area_name}}/{{area}}{{flow}}Tests.cs`)

Also list files to be **created** and files to be **modified** (page-objects included).

**Gate:** output Part 1 + Part 2 in chat and STOP. Ask the user to approve or request modifications. On modifications → update the plan, re-present, wait again. Create/modify any file ONLY after explicit approval. Never jump straight from the design to code.

### Step 3: Create/extend the page-object

**Mandatory rules reading (before writing any code):** open and read the full list of project rules — do not rely on memory or on digests duplicated in AGENTS.md:
- `Rules/ui-testing.md`
- `Rules/code.md`, `Rules/code-style.md`, `Rules/code-principles.md`
- `Rules/models.md`, `Rules/assertions.md`
- `Rules/test-practices.md`, `Rules/categories.md`
- `Rules/comments.md`, `Rules/config.md`
- `Rules/workflow.md`, `Rules/git.md`

`Rules/localization-testing.md` applies only when the flow belongs to a localization scope — otherwise skip it. If any rule here contradicts this skill, the Rules file wins — flag the contradiction to the user instead of silently choosing one.

Generated page-objects and tests must match the approved plan from Step 2 — no extra methods, no dropped tests.

In `Ui/github/Pages/GitHubStream/{{area_name}}/{{area_name}}Page.cs`:

```csharp
internal class {{area_name}}Page
{
    private readonly Page _page;   // or base-class browser reference per existing pages

    internal async Task SomeBusinessAction(string title, string body)
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "New issue" }).ClickAsync();
        await _page.GetByLabel("Title").FillAsync(title);
        await _page.GetByLabel("Body").FillAsync(body);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Submit new issue" }).ClickAsync();
    }
}
```

- Methods = business actions (`OpenIssuesTab`, `CreateIssue`, `CloseIssue`), never raw clicks exposed to tests.
- Locators inline per action (page under test = GitHub → role/text only); keep one locator per element, named by role+name.
- If a base page-class pattern already exists under `Ui/github/Pages/` — inherit it, do not reinvent.
- Register new page in `BrowserPlaywright` wrapper if the wrapper exposes pages as properties (follow the existing pattern).

### Step 4: Create the test class

```csharp
namespace Ui.Github.Tests.GitHubStream.{{area_name}};

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class {{area_name}}{{flow_name}}Tests : {{service_name}}UiTestBase
{
    private readonly List<long> _createdIssueNumbers = [];   // registry, typed per resource

    [Test]
    [Category("Regression")]
    [Description("UI-{{section}} {{flow description}}")]
    public async Task {{area}}_{{Flow}}_{{Outcome}}()
    {
        var title = $"UI {DataGenerator.RandomString(TestDataConstants.TitleRandomLength)}";

        await Browser.SomePage.CreateIssue(title, body);
        // register-then-assert for every created resource:
        // id resolved via UI or API read-back → add to registry BEFORE content asserts

        // API cross-verify (preferred oracle) — compose Core RequestHelper on the UI base
        // (status assert BEFORE .Data dereference, per Rules/test-practices):
        // var created = await _gitHubRequestHelper.GetIssuesAsync(...);
        // created.ShouldHaveStatusCode(HttpStatusCode.OK);
        // created.Data.Should().Contain(issue => issue.Title == title);

        // UI-only asserts (what API cannot see): badges, URL, empty-state text
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // cleanup via Core API by composition — close issue / delete comment;
        // warning-only semantics, never fails the run
    }
}
```

- Title/body via `DataGenerator.RandomString` (fictional values, vary ≥2 dimensions) — see `Rules/test-practices.md` → Test data. Lengths and shared test-data values come from `TestDataConstants` (`Core/Helpers/Data/`) — never redeclare them locally.
- `[OneTimeTearDown]` is the LAST class member; `[OneTimeSetUp]`/fields at top.
- No `[Category]` check-type on the class; no service category on methods.
- Do not paste screenshot/trace code — the base fixture captures artifacts on failure.
- UI base has **no** `Get<>` / `ShouldHaveStatusCode` / `CleanupIssueAsync` members — wire Core helpers by composition (or extend the fixture) before using them in tests.

**Negative example — a UI test that must NEVER be generated (and why):**
```csharp
[Test]
public void CreateIssueTest()
{
    _page.ClickAsync(".css-1x6g8xn");
    _page.FillAsync("#title", "test");
    _page.WaitForTimeoutAsync(3000);
    Assert.IsTrue(_page.Url.Contains("issues"));
}
```
Why it is rejected: CSS selector invented from memory (hard rule 1 — role/text locators from a live `locator-dump` only); `WaitForTimeout` sleep (hard rule 4 — Playwright auto-waits, poll via `WaitHelper` for documented eventual consistency); login likely repeated in-test (hard rule 2 — `storageState`, login only in `LoginSignInTests`); NUnit `Assert.*` (FluentAssertions only); no register-then-assert for the created issue (hard rule 3 — UI write → API cleanup); no `[Category]`/`[Description]`; static "test" data.

### Step 5: Sync UI Observable Behaviour

Append/extend `documentation/{{service_name}}UiObservableBehaviour.md` for every page touched:
- Section = page; bullets = verified locators (`GetByRole(Button, "New issue")` — verified {date}), URL pattern, states (enabled/badge text), gotchas (eventual list refresh, device-verification wall).
- Only snapshot-verified facts; undated claims are drafts.

### Step 6: Safety check

**Plan-vs-implementation reconciliation (mandatory, first):** compare the generated code against the plan approved in Step 2. Every planned test method (and planned page-object file) must exist; no unplanned tests may appear. Output a table:

| # | Planned test (method name) | Category | In code | Status |
|---|---|---|---|---|
| 1 | `Issues_Create_AppearsInList` | Regression | ✅ | matched |

Mismatches (renamed / missing / extra method) → fix the code to match the plan (or, if the change was requested by the user during generation, update the plan first) → re-run the reconciliation until every row is `matched`. Proceed below only when the table is fully green.

Then:
1. `dotnet format --verify-no-changes`
2. `dotnet test --filter "Category=GitHubUi&FullyQualifiedName~{{area_name}}{{flow_name}}"` — new tests pass
3. Hygiene: `git status` — no `.auth/`, no `Ui/bin|obj`, no screenshots staged

**Test run report (mandatory, after the run):** build a per-test table from the run results:

| # | Test method | Category | Result | Duration | Error (if failed) |
|---|---|---|---|---|---|
| 1 | `Issues_Create_AppearsInList` | Regression | ✅ passed | 4.5s | — |
| 2 | `Issues_Create_MissingTitle_ShowsError` | Negative | ❌ failed | 3.1s | expected error text, form submitted |

0 failed → continue to Step 7. ≥1 failed → fix and re-run (or `[Ignore]` with explanation for a documented bug) — the run must end green before the report step. `safetyCheck.testRun` in the JSON contract is filled from this table.

### Step 7: Report

Return the JSON contract from "Output Format Instruction" (`files` / `tests` / `locators` / `safetyCheck` / `suggestedCommitMessage`) after validating rules 1–5. The plan-vs-implementation reconciliation table and the test run report table from Step 6 are **required** parts of the reply — include them alongside the JSON (before or after, not omitted). A human-readable summary may follow — the JSON + both tables are the deliverable, the prose is optional.

---

## Examples

**User says:** «сгенерируй UI тест для создания issue через UI»
→ Step 1: read `GitHubUiObservableBehaviour.md` §Issues + snapshot `github.com/Gredja/AiTest/issues/new` → extract roles/labels
→ Step 3: `IssuesPage.CreateIssue(title, body)`
→ Step 4: `IssueCreationTests.Issue_Create_AppearsInList` (Regression; API `Contain(Title)` cross-verify; registry → API close in teardown)
→ Step 5–7: OB section updated, format+filter run, report

**User says:** «добавь проверку страницы PR»
→ extend existing `PullsPage` if present (Step 1.0!) — never a second `PullsPage`

---

## Troubleshooting

| Error / symptom | Cause | Fix |
|---|---|---|
| `TimeoutException: locator not resolved` | stale selector or wrong role | re-snapshot the page; locator from live DOM only |
| Test hangs on navigation | missing storageState / expired session | run `dotnet test --filter LoginSignInTests` to refresh `.auth/github-ui-state.json` |
| Flaky list-visibility assert | eventual list refresh | poll with `WaitHelper.WaitUntilAsync` (documented in UI-OB section), never `WaitForTimeout` |
| `StorageStatePath not found` | login test never ran | tell user to run `dotnet test --filter LoginSignInTests` once (credentials in env/.env) |
| Test fails, no artifact | fixture not capturing | fix `UiTestBase` teardown (screenshot+trace) — not the test |
| CSS selector temptation (`div.Box-row`) | shortcut | forbidden — role/text locator + snapshot verification |

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-10-09 | Initial: XRM_Autotest-inspired structure, live-snapshot locator rule, UI-write→API-cleanup, storageState auth | MiMo |
| 1.1 | 2026-11-04 | Machine-checkable JSON report (incl. locators.draft=0 invariant), mandatory plan gate (plan → approval → files), negative example (rejected UI test with violation list) | MiMo |
| 1.2 | 2026-11-05 | Step 2 gate → Part 1 plan table + Part 2 full test-method list (names/categories/descriptions/files); Step 3: mandatory Rules/*.md reading + match-approved-plan; Step 6: reconciliation + per-test run report tables (green before Step 7); Step 7: both tables required alongside JSON | MiMo |
