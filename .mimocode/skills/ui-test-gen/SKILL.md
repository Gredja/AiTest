---
name: ui-test-gen
description: Use when the user wants to generate UI tests for a web interface with Playwright (.NET + NUnit) in the Gredja Ui project. Trigger on mentions of "ui test gen", "generate UI tests", "/ui-test-gen", "сгенерируй UI тесты", testing a new page, screen or browser flow (navigation, create via UI, issue forms). Not for API tests (use api-test-gen) and not for API scenario chains (use e2e-test-gen).
---

# UI Test Generation (Playwright)

## Purpose

Generates page-objects and NUnit UI tests for the Gredja `Ui/` project (Microsoft.Playwright + Chromium) — for AQA Engineer at the test automation stage. Design follows the XRM_Autotest reference: page-objects expose business actions, tests orchestrate and verify.

---

## Important (hard rules — read before anything else)

1. **Locators are role/text/label-based only** — `GetByRole`, `GetByText`, `GetByLabel`, `GetByPlaceholder`. NEVER CSS-classes or XPath on the page under test (GitHub generates hashed BEM-classes that change between releases). Every locator must be verified against a **live snapshot** of the page before it is written (see Step 1).
2. **No login/logout in tests** — the fixture loads `storageState` from `.auth/github-ui-state.json` (gitignored). A logout test breaks every parallel colleague; login is the `[Explicit]` bootstrap test only.
3. **UI write → API cleanup** — resources created through the UI are removed in `[OneTimeTearDown]` (LAST member of the class) via the Core API helpers (`CleanupCommentAsync`, `CleanupIssueAsync` pattern: `RunCleanupAsync` = warning, never fail the run). Register in the registry immediately after creation, BEFORE any assert (register-then-assert).
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

Model must return: page-object files (.cs) and test files (.cs) in C# with namespaces, classes, methods. Code without comments (comments only for non-obvious WHY). After generation — report: list of files, test count by category, and the verification commands run.

---

## Input

User provides:
1. **Service** — `{{service_name}}` (default `GitHub` — must have `Ui/` project)
2. **Area / page** — `{{area_name}}` (e.g. `issues`, `repo landing`)
3. **Flow to cover** — what to do and verify on that page (e.g. "create issue via UI, verify it appears in the list")

If any is missing — ask before generating.

---

## Preconditions

- `Ui/` project exists in `Gredja.slnx` (scaffold per `.mimocode/plans/ui-playwright-github.md`). If not — stop, tell the user the scaffold phase must run first.
- `testsettings.json` has `Ui` section; `.auth/` is gitignored; `Scripts/install-playwright.ps1` was executed (chromium installed).
- Rules in context: `Rules/ui-testing.md`, `Rules/categories.md`, `Rules/test-practices.md` (teardown-last, register-then-assert, create→remove), `Rules/code.md`.

## File Convention

| What | Path pattern |
|---|---|
| UI Observable Behaviour | `documentation/{{service_name}}UiObservableBehaviour.md` |
| Base fixture (browser/context/page) | `Ui/Helper/{{service_name}}UiTestBase.cs` |
| Browser wrapper (pages as properties) | `Ui/Helper/BrowserHelpers/BrowserPw.cs` |
| Page-objects | `Ui/Helper/UiHelper/GitHubStream/{{area_name}}/{{area_name}}Page.cs` |
| Shared pages (header, nav) | `Ui/Helper/UiHelper/CommonPages/` |
| Tests | `Ui/Test/GitHubStream/{{area_name}}/{{area_name}}{{flow_name}}Tests.cs` |
| Test data files | `Ui/Data/` |
| Namespace (tests) | `Ui.Test.GitHubStream.{{area_name}}` |
| Namespace (page-objects) | `Ui.Helper.UiHelper.GitHubStream.{{area_name}}` |
| Service category (class) | `[Category("GitHubUi")]` |
| Description reference | UI-OB section number, e.g. `"UI-3.2 Create issue via UI"` |

Before generating — verify the target directories exist; create missing page-object folder only after checking no sibling covers the same page.

---

## Flow

### Step 1: Research the page (live)

0. **Read existing code FIRST** — Glob `Ui/**` for the area's page-object and tests; extend what exists, never duplicate files.
1. **Read UI Observable Behaviour** — `documentation/{{service_name}}UiObservableBehaviour.md`. It is the source of truth: per section — verified locators (with `verified {date}` markers), element states, URL patterns, gotchas. If missing — create it while working: one section per page, locators only after live verification.
2. **Take a live snapshot of the page** — use the `playwright` skill (browser snapshot) against the real page (`Ui.BaseUrl` from `testsettings.json`). Extract roles/texts/labels for every locator you are about to write. Do NOT invent selectors from memory of the HTML.
3. Note URL patterns, navigation path, and what eventual-consistency the page exhibits (list refresh after write?).

### Step 2: Design the test

- Map the flow to one test method (chain = one test, E2E-style) or several single-check tests.
- Pick ONE check-type category per test per `Rules/categories.md` boundaries: Smoke = single-response sanity (content, count); Regression = rule/invariant/state-transition; Negative = error/404/empty states.
- List UI-created resources → plan their API cleanup (issue → `CleanupIssueAsync` close, comment → `CleanupCommentAsync` delete).
- Prefer hybrid verification: **act via UI, assert via API** (Core `RequestHelper`) for created resources — the API read-back is the reliable oracle; UI asserts are for what only exists visually (badges, URLs, empty states).

### Step 3: Create/extend the page-object

In `Ui/Helper/UiHelper/GitHubStream/{{area_name}}/{{area_name}}Page.cs`:

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
- If a base page-class pattern already exists in `Ui/Helper/UiHelper/BaseHelpers/` (XRM-style `CreateUpdatePage` analog) — inherit it, do not reinvent.
- Register new page in `BrowserPw` wrapper if the wrapper exposes pages as properties (follow the existing pattern).

### Step 4: Create the test class

```csharp
namespace Ui.Test.GitHubStream.{{area_name}};

[TestFixture]
[AllureNUnit]
[Category("GitHubUi")]
public class {{area_name}}{{flow_name}}Tests : {{service_name}}UiTestBase
{
    private const int TitleRandomLength = 8;

    private readonly List<long> _createdIssueIds = [];   // registry, typed per resource

    [Test]
    [Category("Regression")]
    [Description("UI-3.2 {{flow description}}")]
    public async Task {{area}}_{{Flow}}_{{Outcome}}()
    {
        var title = $"UI {DataGenerator.RandomString(TitleRandomLength)}";

        await Browser.SomePage.CreateIssue(title, body);
        // register-then-assert for every created resource:
        // id resolved via API read-back → add to registry BEFORE content asserts

        // API cross-verify (preferred oracle):
        var created = await Get<List<IssueModelResponse>>(GitHubEndpoints.RepoIssues, TestRepoParam());
        created.ShouldHaveStatusCode(HttpStatusCode.OK);
        created.Data.Should().Contain(issue => issue.Title == title);

        // UI-only asserts (what API cannot see): badges, URL, empty-state text
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        foreach (var id in _createdIssueIds)
        {
            await CleanupIssueAsync(id);   // warning-only, never fails the run
        }
    }
}
```

- Title/body via `DataGenerator.RandomString` (fictional values, vary ≥2 dimensions) — see `Rules/test-practices.md` → Test data.
- `[OneTimeTearDown]` is the LAST class member; `[OneTimeSetUp]`/fields at top.
- No `[Category]` check-type on the class; no service category on methods.
- Do not paste screenshot/trace code — the base fixture captures artifacts on failure.

### Step 5: Sync UI Observable Behaviour

Append/extend `documentation/{{service_name}}UiObservableBehaviour.md` for every page touched:
- Section = page; bullets = verified locators (`GetByRole(Button, "New issue")` — verified {date}), URL pattern, states (enabled/badge text), gotchas (eventual list refresh, device-verification wall).
- Only snapshot-verified facts; undated claims are drafts.

### Step 6: Safety check

1. `dotnet format --verify-no-changes`
2. `dotnet test --filter "Category=GitHubUi&FullyQualifiedName~{{area_name}}{{flow_name}}"` — new tests pass
3. Hygiene: `git status` — no `.auth/`, no `Ui/bin|obj`, no screenshots staged

### Step 7: Report

- Files created/modified; test count by category
- Locators verified live (page + date) vs still draft
- Verification results; suggested commit message

---

## Examples

**User says:** «сгенерируй UI тест для создания issue через UI»
→ Step 1: read `GitHubUiObservableBehaviour.md` §Issues + snapshot `github.com/Gredja/AiTest/issues/new` → extract roles/labels
→ Step 3: `IssuesPage.CreateIssue(title, body)`
→ Step 4: `IssueCreationTests.Issue_Create_AppearsInList` (Regression; API `Contain(Title)` cross-verify; registry → `CleanupIssueAsync`)
→ Step 5–7: OB section updated, format+filter run, report

**User says:** «добавь проверку страницы PR»
→ extend existing `PullsPage` if present (Step 1.0!) — never a second `PullsPage`

---

## Troubleshooting

| Error / symptom | Cause | Fix |
|---|---|---|
| `TimeoutException: locator not resolved` | stale selector or wrong role | re-snapshot the page; locator from live DOM only |
| Test hangs on navigation | missing storageState / expired session | run `[Explicit]` bootstrap test to refresh `.auth/github-ui-state.json` |
| Flaky list-visibility assert | eventual list refresh | poll with `WaitHelper.WaitUntilAsync` (documented in UI-OB section), never `WaitForTimeout` |
| `StorageStatePath not found` | bootstrap never ran | tell user to run `dotnet test --filter StorageStateBootstrap` once, headed, log in manually |
| Test fails, no artifact | fixture not capturing | fix `UiTestBase` teardown (screenshot+trace) — not the test |
| CSS selector temptation (`div.Box-row`) | shortcut | forbidden — role/text locator + snapshot verification |

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-10-09 | Initial: XRM_Autotest-inspired structure, live-snapshot locator rule, UI-write→API-cleanup, storageState auth | MiMo |
