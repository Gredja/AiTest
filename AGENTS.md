# AGENTS.md — Gredja

AQA project. API tests (NUnit + RestSharp): JSONPlaceholder, GitHub API.
E2E tests (NUnit + RestSharp): GitHub scenario chains (multi-step write flows).

## Project Structure

- `Api/` — NUnit API tests (JsonPlaceholder, GitHub), including single write tests
- `Ui/` — NUnit UI tests (Playwright .NET + Chromium): site scope — the `Ui/github/` folder (future sites — sibling folders, e.g. `Ui/moodledemo/`); pages/forms — `Ui/github/Pages/` (shared common chrome — `Ui/github/Pages/Common/`, family base classes — `RepositoryPageBase`), infrastructure — `Ui/github/Tests/Support/`, tests — `Ui/github/Tests/`; authorization via storageState (`.auth/github-ui-state.json`): the state is refreshed by the automated test `LoginSignInTests` (login via `GITHUB_UI_EMAIL`/`GITHUB_UI_PASSWORD`), the other tests only read the state; write via UI → cleanup via API; locators ONLY on Page/Form (at the bottom of the file), helpers receive `ILocator` as parameters; rules — `Rules/ui-testing.md`. The `Ui/moodledemo/` sibling covers the Moodle demo multilingual site (`https://school.moodledemo.net`): HTTP-first L1 locale smoke + Playwright checks (language switcher, persistence, RTL layout, login localization), anonymous contexts only (no storageState), locale expectations from `Ui/moodledemo/Fixtures/expected-langs.json`, rules — `Rules/localization-testing.md`, category `MoodleDemo`
- `E2E/` — NUnit E2E tests: scenario chains only (create → verify → mutate → cleanup)
- `Core/Models/` — response/request models
- `Core/Helpers/` — shared helpers by subfolder: `Http/` (request infrastructure: `RequestHelper`, `JsonPlaceholder/GitHubRequestHelper`), `Data/` (`DataGenerator` — varied write data), `Assertions/` (`AssertHelper`), `Params/` (`ParamHelper`), `Waiting/` (`WaitHelper`, `WaitResult`); needed for E2E → `GitHub/` (`GitHubTestBase`, `GitHubParamHelper`); API tests only → `Api/<Service>/Helpers/`
- `Core/Attributes/` — model-field validation attributes (`PositiveIdAttribute`, `RequiredFieldAttribute`, `ValueRangeAttribute`)
- `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs`, `Core/Config/MoodledemoEndpoints.cs` — URLs and endpoint paths
- `Core/Logging/` — action logging (Serilog → `%TEMP%\GredjaTestRun\actions-*.log`) and raw test results (`test-results-*.log`). `TestResults/` contains **reports only**, raw run artifacts live in the temp folder
- `Core/Reporting/` — `TestRunReportGenerator` → `TestResults/TestRunReport-*.md` (invoked from the NUnit teardown `TestReportSetup`), age-based cleanup of artifacts older than 7 days
- `AllureAdapter/` — solution project: Allure NUnit attribute + JSON writers/report helpers (`AllureAdapter/Helpers/`, flat layout by design)
- `Scripts/` — one-off helper scripts: `locator-dump.mjs` (CDP locator dump), `normalize-eol.ps1`, allure/test runners
- `Ui/Data/` — static test files (upload payloads; reserved, currently empty)
- `Ui/Support/` — project-level UI helpers shared by the site trees (`ArtifactNaming`)
- `Rules/` — full rules (this file is only a brief digest)
- `documentation/` — project documentation; `documentation/Katas/` — learning materials, do not touch
- `documentation/*ObservableBehaviour.md` — observable behavior of the API (current states, field types, negative cases). **Must be read when generating and reviewing tests.**
- `education/TESTING_PLAYBOOK.md` — portable project-agnostic testing methodology (EN, fed to other agents/projects); update it with every commit (skill `/commit`, Step 6)
- `.mimocode/skills/` — skills for AI agents
- `.github/workflows/` — CI: `pr-checks.yml` (required PR gate: format always, HealthCheck for same-repo branches) and `nightly-tests.yml` (nightly informational run); secrets live only in the repo settings, the live suite is never a required check (`Rules/git.md` → CI)

## Rules

### Workflow
- **Session start:** right after the greeting, before any other work — run the `/nightly-check` skill (one-line verdict on the `Gredja/AiTest` nightly run). Never skip it, even for a "quick question" — verdict first, then the user's work (duplicate of project MEMORY → Session start; this is the canonical text present in every session context).
- **Documentation language (hard rule, 2026-10-19):** repository-facing documents — **English only** (`Rules/*`, `education/TESTING_PLAYBOOK.md`, `documentation/*` including OB and templates, `.mimocode/skills/*/SKILL.md`, `README.md`, `AGENTS.md` itself). **Exception: `backups/SETUP.md` stays Russian** — it is the onboarding guide for the RU-speaking user and the only RU repository document. User-facing chat replies — **Russian**. Russian text inside English files is allowed only as quoted user trigger phrases or rule citations (e.g. skill description triggers), never as the main body. Mixed language without a quoted-source reason = a review violation.
All changes: plan → approval → report. After a commit — review `Rules/`. After structural changes — update the documentation AND tracked `backups/` (including removal of gitignored files — in the same pass, even without a commit). Completed plans (`.mimocode/plans/`) — delete right after implementation. A rule expressed as IDE0xxx → `.editorconfig` in the same change; a new rule wording is checked against the existing `Rules/*.md` for contradictions.

### Code
- Naming: PascalCase (classes, methods, properties, constants), camelCase (locals, params), `_camelCase` (private fields)
- Before creating any file, decide its place by common sense and future plans (`Rules/code.md` → File Layout)
- Lambda parameters: readable singular noun (`product => product.Id`), not single-letter (`p =>`)
- No abbreviations (`response`, not `resp`). Booleans: `Is`, `Has`, `Can`, `Should`
- File-scoped namespaces, one class per file, use `var` wherever possible (explicit type only when `var` is not applicable, e.g. `null`, tuples)
- No dead `using` — remove them in the same pass as the code (caught by `dotnet format` via `.editorconfig` IDE0005)
- Methods: short, one responsibility, max ~30 lines (test methods may be longer if splitting would hide the scenario — `Rules/code.md`), max 5 params
- All API requests are async (`ExecuteAsync`, not `Execute`)
- No modifier = private. No magic numbers. No nested ternaries. `nameof()` for exceptions
- Error handling: concrete exceptions, no `null!`, no exceptions for flow control
- LINQ: `Any()` instead of `Count() > 0`, no redundant `.ToList()`, `FirstOrDefault()` instead of `Where().FirstOrDefault()`; synchronous side-effect loops on `List<T>` → `.ForEach(item => ...)` (async/lazy sources → `foreach`, `Rules/code-style.md` → Loops)
- Strings: interpolation `$""`, `string.Empty` over `""` (except where a compile-time constant is required), `StringBuilder` in loops, `IsNullOrEmpty()` instead of `.Length == 0`
- Null safety: `?.` for safe navigation, `??` for fallback, `is not null` instead of `!= null`
- SOLID: one class — one responsibility, dependencies via interfaces, extensibility via inheritance; **accepted debt: the test project deliberately has zero hand-rolled interfaces/DI** (`Rules/code-principles.md` → DIP note, audit F09) — no "no interfaces" findings, no checklist DI refactors
- Test classes (`*Tests.cs`): `[Test]` + lifecycle + test data only; shared parameter builders live in base/helpers, not in the test class; cleanup only in `[OneTimeTearDown]` (**the last member of the class**) via the fixture registry (register-then-assert), not in `finally`; **created → deleted, no keeping test records** (no delete API → nearest cleanup: close issue/PR) (`Rules/test-practices.md`)

### Models
- Response: suffix `ModelResponse` (includes `Id`). Request: suffix `ModelRequest` (no `Id`). Nested/auxiliary — no suffix
- Reference types (string, object, List): no `?`, no initializer
- Value types (int, decimal, DateTime): `?` only if the JSON field can be null/absent
- Namespace: `Core.Models.{Service}` (e.g. `Core.Models.GitHub`). Pure data containers — no constructors, validation, or logic
- JSON names: only the `JsonFields` constants of their own domain (`Core/Models/{Service}/JsonFields.cs`) — no inline literals, no copies in test helpers

### Assertions
- FluentAssertions (not NUnit Assert)
- Key: `.Should().Be()`, `.NotBeNull()`, `.NotBeNullOrWhiteSpace()`, `.BeGreaterThan()`, `.BeInRange()`, `.OnlyContain()`
- Helper methods: `ShouldHaveStatusCode()`, `ShouldHaveError()`, `ShouldHaveValidContract()`, `ShouldHaveValidFields()`, `ShouldMatchRequest()` — in `AssertHelper.cs`

### Comments
- By default: no comments. The code speaks for itself.
- A comment is a last resort: first refactor for clarity; a comment only if the code can no longer be refactored and is still unclear (`Rules/comments.md`).
- Exceptions: regex explanations, TODO (only in dev, remove before merge), non-obvious WHY

### Config
- Base URL and endpoints live in `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs`. Never hardcode them in tests.
- `testsettings.example.json` stays in sync with `testsettings.json`; token: JSON `Token` → env `GITHUB_PAT` → `.env` (`Rules/config.md`); do not mix the two GitHub accounts: API/E2E = `Gredja`, UI = `gredjapl` (`Rules/config.md` → GitHub identities)

### Git
- Repo: github.com/Gredja/AiTest.git, branch: `main`
- Make changes in `features/<topic>` branches
- Commits: ONLY via the `/commit` skill (never by hand); only on request, English, format: action + object
- Never commit `.env` or tokens
- **Before committing:** `dotnet format --verify-no-changes` + `dotnet test --filter Category=HealthCheck` — both must pass (full run — `/test`)
- CI: PR gate (`format` + `HealthCheck`) and nightly informational run; never make the live suite a required check (`Rules/git.md` → CI)

## Key Decisions
- 0 and -1 — safe static IDs (always invalid)
- Dynamic non-existent ID = maxId + 1 (not a static 999); for concurrent writes in the same run (Api + E2E solution run) — maxId + Offset (`GitHubEndpoints.NonExistentIdOffset = 100`)
- Seed methodology: 5 seeds → expand → enforce 5+ active negatives (mock-API exception + ceiling rule) → see `Rules/test-practices.md`
- Document sync: Observable Behaviour ↔ Test Plan ↔ Rules — always in sync
- Guarantee Data: GET empty → POST in OneTimeSetUp → GET again → Assertion → DELETE in OneTimeTearDown
- Test data for write: only fictional values (E2E writes to a public repo), vary ≥2 dimensions, obfuscation via replacement — see `Rules/test-practices.md` → "Test data for write operations"

## Detailed Rules (Rules/)

- `Rules/code.md` — naming, types, file structure, methods, async, access modifiers, cleanup
- `Rules/code-style.md` — error handling, LINQ, strings, null safety
- `Rules/code-principles.md` — SOLID, general principles
- `Rules/models.md` — model building rules (ModelResponse/ModelRequest, properties, naming)
- `Rules/assertions.md` — FluentAssertions, key patterns, HTTP response assertions, request/response comparison
- `Rules/test-practices.md` — test isolation, API patterns, non-existent IDs, E2E cleanup, artifact cleanup, seed methodology, document sync (incl. single-source guidance), read-after-write visibility
- `Rules/comments.md` — when comments are needed in code
- `Rules/config.md` — endpoints, configuration
- `Rules/git.md` — remote, commits, secrets, repo hygiene, CI
- `Rules/workflow.md` — plan → approval → changes → report
- `Rules/categories.md` — test categories
- `Rules/ui-testing.md` — UI tests (Playwright): page-object structure, locators, auth/storageState, waits (waiting for the outcome of the action before assert), cleanup, categories
- `Rules/localization-testing.md` — localization testing (Moodle demo): language-agnostic locators (L-LOC-1), locale contract (`html lang/dir`, fixture titles), HTTP-first L1, data-driven languages, demo-environment safety

## AI Onboarding (for beginners)

To start working with AI in this project without any explanations:

1. Read `AGENTS.md` in full and `Rules/test-practices.md` — they contain the basic rules and the seed methodology
2. `/api-test-gen <service> <endpoint>` — generate tests for a new endpoint (the skill will read Observable Behaviour and the rules itself)
3. `node Scripts/locator-dump.mjs <url> [--waitfor "text"]` — one-off dump of role + accessible name from a page for UI locators (CDP, no dependencies; uses `.auth/github-ui-state.json` by default, `--anonymous` — without sign-in)
4. `/test` — run the whole test suite (writes `TestResults/TestRunReport-*.md`), `/test-report` — run + Allure report
5. `/review-commit` — check uncommitted changes against the project rules
6. `/commit` — formatting + HealthCheck + commit + push

Full list of skills: `.mimocode/skills/`. All changes follow the cycle: plan → approval → changes → report (`Rules/workflow.md`).

## Knowledge Graph

```bash
$env:PATH = "C:\Users\User\.local\bin;$env:PATH"
graphify . --code-only --force
graphify query "show all models"
graphify path "PostModelResponse" "User"
graphify explain "UserModelResponse"
```
