# Rules: UI Testing (Playwright)

## Structure (XRM_Autotest-inspired)

```
Ui/
├── Ui.csproj                     # Microsoft.Playwright + NUnit, refs Core + AllureAdapter
├── AssemblyInfo.cs               # Parallelizable(Fixtures), LevelOfParallelism(1)
├── TestReportSetup.cs            # [SetUpFixture] — duplicated per test assembly (P2.3)
├── github/                       # site-scoped: все UI-тесты GitHub (будущие сайты — соседние папки)
│   ├── Pages/                      # единственный дом локаторов
│   │   ├── Common/                   # cross-area pages (header, nav)
│   │   └── GitHubStream/             # page-objects: {Area}/{Area}Page.cs
│   └── Tests/
│       ├── Support/                  # GitHubUiTestBase, BrowserPlaywright, TabCountParser
│       └── GitHubStream/             # tests: {Area}/{Area}{Flow}Tests.cs
└── Data/                         # static test files (upload payloads)
```

- Page-object method = business action (`CreateIssue(title, body)`), never a raw click
- Test classes contain only tests — actions in page-objects, browser logic in the fixture
- Register every new page-object as a property on `BrowserPlaywright` (wrapper pattern)

## Locators

- **Role/text/label only**: `GetByRole`, `GetByText`, `GetByLabel`, `GetByPlaceholder`
- NEVER CSS-classes or XPath on the tested site (generated class names change between releases)
- **Reuse before extract** — on similar pages, apply locators/patterns already verified on other pages (shared header, tab counts `Open (N)`/`Closed (N)`, level-N headings) instead of snapshotting everything again; snapshot only the page-unique elements, or re-snapshot when an existing locator fails
- New locators are verified against a **live snapshot** before they are written (agent HARD RULE: only via `node Scripts/locator-dump.mjs <url> [--waitfor "text"]` — one-shot CDP dump of role+accessible name; the `playwright` skill / playwright-cli is FORBIDDEN for locator extraction, 2026-10-17); undated claims in the UI Observable Behaviour are drafts
- A role+name locator that never resolves although the control is clearly visible → re-check its **accessible name** in the live snapshot: GitHub injects sr-only text into it (shortcut hint renders as `Save ( enter )`); match the visible label instead — `GetByText("Save", Exact = true)` (2026-10-14)
- One locator per element; locator lives in the page-object, not in tests
- Locators are separate named entities (constants/fields) collected **at the bottom of the
  page-object file**; methods use only these entities — never inline locators in method bodies
  (2026-10-09)
- **Locators exist ONLY on Page/Form classes** (single source of truth). Helpers and any other
  shared code NEVER declare locators — they RECEIVE them as `ILocator` parameters from the
  page/form that owns them; that removes selector duplication (2026-10-16). Chrome shared by
  many pages (header/nav) lives in a page-object base class, not in helpers; when a helper
  needs many elements, pass a small grouped object rather than growing the signature past
  ~5 params

## Auth (storageState)

- **All UI tests use the `gredjapl` account** (fresh dedicated UI account; API tests keep `Gredja` via `GitHub.TestUsername`). Assert username via `TestConfig.UiUsername` (`Ui.Username` in testsettings.json)
- Feature tests read `.auth/github-ui-state.json` (gitignored) — they never log in or log out (a logout breaks every other test in the run)
- State is refreshed by the automated Smoke test `LoginSignInTests` (`dotnet test --filter LoginSignInTests`): anonymous context (`IsStorageStateEnabled => false`) → sign in with `GITHUB_UI_EMAIL` / `GITHUB_UI_PASSWORD` → assert signed-in menu → `SaveStorageStateAsync()`
- The logout test (`LogoutSignOutTests`, UI-2) is the only test that signs out: it runs with `IsStorageStateEnabled => false` and never saves state. `/logout` opens an account chooser — click only the per-account `Sign out of {username}` button; the sibling `Sign out from all accounts` destroys the shared storageState session and breaks every other UI test
- Credentials are required env vars (`.env` / environment): never paste passwords into tests, code, or config files
- Expired state = re-run `LoginSignInTests`

## Waits

- Playwright auto-waiting is the default — no `WaitForTimeout` without a WHY-comment
- **Wait for the outcome of an action before the next step.** Auto-waiting covers the actionability of the *next* action only — never the outcome of the *previous* one. When an action has a network outcome (submit → redirect, appended item, status flip, leaving edit mode), the page-object method waits for its post-condition (`WaitForURLAsync` / `WaitForAsync` on the resulting element) **before it returns**; assertions and the next step run after that. A `CountAsync()` check right after such a click is a race — proven: create/comment/save asserted while the POST was still in flight
- Pattern for new tests: click → method waits for the element that exists only after the outcome → test asserts. Ambiguity rule: wait for a locator that cannot match before the outcome (button swap `Close issue` → `Reopen issue`), not for text that may already be on the page
- An effect that can be swallowed (click landing before the header is wired up) → re-apply the action **bounded** until the post-condition holds, via `WaitHelper.WaitUntilAsync` with the state check **before** every retry — so an element that merely renders late is never toggled shut
- Retry interval must exceed the effect's render latency: the user menu renders ~300ms after the click, so a 250ms retry re-clicked an already open menu, whose trigger then loses its accessible name and burns the whole 15s action timeout (2026-10-14)
- Navigation is covered differently: `OpenAsync` relies on `GotoAsync` (waits for `load`) and GitHub server-renders list/page content (verified 2026-10-13: repo root, `tree/main`, issues list incl. empty state, pulls list incl. empty state), so an instant check after `OpenAsync` is not a race. Content that hydrates later (buttons/forms on the issue page) is already covered by auto-wait of the action that uses it. A presence check on a page **outside** that verified list waits in the page-object instead of reading the count instantly: `WaitFor*Async` → `WaitResult<bool>` with `attempts`/`elapsed` in the assert message (e.g. `IssueCreatePage.WaitForOpenAsync`, `NotFoundPage.WaitFor404MessageAsync`)
- Documented eventual consistency (list refresh after a write) → poll with `WaitHelper.WaitUntilAsync` and include `Elapsed`/`Attempts` in the failure message
- Fixture sets 15s default action/navigation timeout (`UiTimeoutMs`)

## Cleanup (UI write → API cleanup)

- Every resource created through the UI is registered **before any assert** (register-then-assert) and removed in `[OneTimeTearDown]` — the LAST class member
- Cleanup via composition: `GitHubUiTestBase.Cleanup` is a `GitHubCleanupHelper` (Core) — same warning-only semantics as Api/E2E (`RunCleanupAsync` never fails the run); issue → close via PATCH (OB §17), comment → API DELETE
- Create → remove, never retain test records (sandbox repo)
- The UI rarely exposes the resource id — read it back via API (`Cleanup.Get` matching the unique title/body) and register it **before** the assert; poll with `WaitHelper`, the API index lags the write (2026-10-14)
- "Fresh sandbox" / empty-state tests assert a **data precondition**: a leftover open issue — or a resource whose registration was skipped by an earlier failed assert — turns them red; REST has no issue delete, so cleanup closes issues — close the leftovers to restore the precondition

## Artifacts

- On failure the fixture saves `TestResults/ui-artifacts/{test-name}.png` (full page) and `.zip` (Playwright trace with snapshots)
- `TestResults/` is covered by the existing age-based cleanup (7 days) — no new registration needed; the directory is gitignored
- Tests never encode screenshot/trace calls — that is fixture code
- Triage order: failure message in `%TEMP%\GredjaTestRun\test-results-*.log` → `.png`/`.zip` in `TestResults/ui-artifacts/` → `trace.trace` inside the zip (action timeline + DOM snapshot around every click)

## Categories and descriptions

- Service category on class: `[Category("GitHubUi")]` (exactly one; check-type categories only on methods)
- Check-type per `Rules/categories.md` boundaries: Smoke = single-view sanity, Regression = rule/invariant/state-transition, Negative = error/404/empty states
- `[Description]` references the UI Observable Behaviour section, e.g. `"UI-1 Ordinary login…"` (every section is numbered `UI-N` in document order — see GitHubUiObservableBehaviour)

## Verification (after every generation)

1. `dotnet format --verify-no-changes`
2. `dotnet test --filter "Category=GitHubUi&FullyQualifiedName~{TestClass}"` — new tests pass
3. Hygiene: no `.auth/`, no `Ui/bin|obj`, no `TestResults/ui-artifacts` files staged
