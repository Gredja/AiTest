# Rules: UI Testing (Playwright)

## Structure (XRM_Autotest-inspired)

```
Ui/
├── Ui.csproj                     # Microsoft.Playwright + NUnit, refs Core + AllureAdapter
├── AssemblyInfo.cs               # Parallelizable(Fixtures), LevelOfParallelism(1)
├── TestReportSetup.cs            # [SetUpFixture] — duplicated per test assembly (P2.3)
├── Helper/
│   ├── GitHubUiTestBase.cs       # browser fixture: Chromium → context(storageState) → Page; artifacts on failure
│   ├── BrowserHelpers/BrowserPw.cs  # page-object wrapper: pages exposed as properties
│   └── UiHelper/
│       ├── CommonPages/          # cross-area pages (header, nav)
│       ├── BaseHelpers/          # shared page base classes (list/create forms)
│       └── GitHubStream/         # page-objects: {Area}/{Area}Page.cs
├── Data/                         # static test files (upload payloads)
└── Test/GitHubStream/            # tests: {Area}/{Area}{Flow}Tests.cs
```

- Page-object method = business action (`CreateIssue(title, body)`), never a raw click
- Test classes contain only tests — actions in page-objects, browser logic in the fixture
- Register every new page-object as a property on `BrowserPw` (wrapper pattern)

## Locators

- **Role/text/label only**: `GetByRole`, `GetByText`, `GetByLabel`, `GetByPlaceholder`
- NEVER CSS-classes or XPath on the tested site (generated class names change between releases)
- Every locator is verified against a **live snapshot** of the page before it is written (agent: use the `playwright` skill); undated selector claims in the UI Observable Behaviour are drafts
- One locator per element; locator lives in the page-object, not in tests

## Auth (storageState)

- Tests read `.auth/github-ui-state.json` (gitignored) — they never log in or log out (a logout breaks every other test in the run)
- The one-time manual login is the `[Explicit]` bootstrap test (`dotnet test --filter StorageStateBootstrap`): headed window → human logs in (2FA/device check included) → state file saved
- Expired state = re-run bootstrap; never paste passwords into tests, code, or config (`GITHUB_UI_PASSWORD` lives in `.env` only if a password flow is ever added)

## Waits

- Playwright auto-waiting is the default — no `WaitForTimeout` without a WHY-comment
- Documented eventual consistency (list refresh after a write) → poll with `WaitHelper.WaitUntilAsync` and include `Elapsed`/`Attempts` in the failure message
- Fixture sets 15s default action/navigation timeout (`UiTimeoutMs`)

## Cleanup (UI write → API cleanup)

- Every resource created through the UI is registered **before any assert** (register-then-assert) and removed in `[OneTimeTearDown]` — the LAST class member
- Cleanup goes through the Core API helpers (`CleanupIssueAsync`, `CleanupCommentAsync` → `RunCleanupAsync`): warning, never fail the run; GitHub issues cannot be deleted → close via PATCH (OB §17)
- Create → remove, never retain test records (sandbox repo)

## Artifacts

- On failure the fixture saves `TestResults/ui-artifacts/{test-name}.png` (full page) and `.zip` (Playwright trace with snapshots)
- `TestResults/` is covered by the existing age-based cleanup (7 days) — no new registration needed; the directory is gitignored
- Tests never encode screenshot/trace calls — that is fixture code

## Categories and descriptions

- Service category on class: `[Category("GitHubUi")]` (exactly one; check-type categories only on methods)
- Check-type per `Rules/categories.md` boundaries: Smoke = single-view sanity, Regression = rule/invariant/state-transition, Negative = error/404/empty states
- `[Description]` references the UI Observable Behaviour section, e.g. `"UI-3.2 Create issue via UI"`

## Verification (after every generation)

1. `dotnet format --verify-no-changes`
2. `dotnet test --filter "Category=GitHubUi&FullyQualifiedName~{TestClass}"` — new tests pass
3. Hygiene: no `.auth/`, no `Ui/bin|obj`, no `TestResults/ui-artifacts` files staged
