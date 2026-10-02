# General Plan — Gredja

Permanent backlog of the user's wishes for the whole project. This file is NEVER deleted
and NEVER trimmed — completed items stay here with the `DONE` status.

**Status legend:** `TODO` · `IN PROGRESS` · `DONE`

**Rules:**
- Never remove an item — only change its status
- New items are appended with the next number
- Status history for each item lives inside the item

---

## 1. Run report: one file per test run — DONE

**Wish (2026-10-02):** every test run — even a single test, and a full suite run too —
produces exactly ONE report file in tabular form: per-test results plus an overall
summary (counts and percentages), with no agent involvement.

**Delivered (final architecture, 2026-10-02):**
- Generation lives in the NUnit teardown `TestReportSetup` (own SetUpFixture in Api + E2E,
  namespace-scoped — independent of the global `AllureGlobalSetup`) — runs for every entry
  point (manual `dotnet test`, VS Test Explorer, future CI) and, critically, **runs even when
  tests fail** (an MSBuild `AfterTargets` hook does not — verified)
- Pure C#, no external runtime: `Core/Logging/TestRunReportGenerator.cs`
- **No Allure dependency anywhere**: statuses/errors come from our own `test-results-*.log`
  (written per test by the assembly-level `TestOutcomeAttribute` ITestAction), actions from
  `actions-*.log`, fixture/category from test sources — Allure can be removed or replaced
  without touching either mechanism
- File: `TestResults/TestRunReport-yyyyMMdd-HHmmss.md` — **TestResults/ holds ONLY report
  files** (gitignored); raw run artifacts (marker, vstest logs, actions, test-results) live
  in `%TEMP%\GredjaTestRun` — one run ⇒ one file in TestResults/
  the name comes from the run-start marker, so both test assemblies of one run write
  the SAME file — exactly one report per run
- Run boundary: `.run-start` marker touched by `Directory.Build.targets` before `VSTest`
  (solution runs start at Core/TestAdapter; standalone `dotnet test Api/Api.csproj`
  detects `SolutionName == '*Undefined*'` and starts its own run) — only logs/results
  newer than the marker are included, so previous runs never leak in
- Sources: test results + failure details (message, stack trace) from our own
  `test-results-*.log`; fixture/category parsed from test sources;
  HTTP actions from `actions-*.log` — all raw logs live in the temp workspace
  `%TEMP%\GredjaTestRun` (wiped at the start of every run)
- Report content: Summary table with counts **and percentages**; per-test table
  (`# | Test | Fixture | Category | Result | Duration`); `## Failed tests` section with
  the full error + stack trace and the complete HTTP action trace of each failed test
- Skipped counter: synthetic `[Ignore]` entries (they carry the ignore reason as
  message) are excluded — the report counts tests that actually executed; the ~29
  ignored tests still show up as Skipped in the Allure report and in `dotnet test` output

**Action logging (2026-10-02):**
- Serilog (`Core/Logging/ActionLogger.cs`) writes every HTTP request/response —
  method, URL, request body, status, response body, duration, current test name —
  to `actions-<timestamp>-<pid>.log` in the temp workspace `%TEMP%\GredjaTestRun`,
  one file per test process
- Single hook point: `RequestHelper.CreateClient` wraps the RestSharp handler via
  `RestClientOptions.ConfigureMessageHandler` — covers all helpers incl. `GitHubTestBase`

**CI readiness (2026-10-02):** no PowerShell, no execution policy, no absolute paths
(repo root found by walking up from the test binaries); CI only needs to archive
`TestResults/` (plus `allure-results/` if the Allure report is wanted).

**Out of scope:** coverage runs (`Scripts/test-coverage.ps1` runs tests internally).

**Status history:**
- 2026-10-02 — `IN PROGRESS`: plan approved, implementation started
- 2026-10-02 — `DONE`: report file delivered and verified (email removed by user decision)
- 2026-10-02 — reworked: PowerShell script replaced with in-process C# generator,
  moved from MSBuild hook (skipped on test failures) to NUnit teardown; added HTTP
  action logging (Serilog) and the failed-test action trace section; run-start marker
  makes the report immune to stale data; one file per run confirmed for solution,
  filtered, failing and standalone-project runs
- 2026-10-02 — decoupled from Allure: own results writer (`TestOutcomeAttribute` →
  `test-results-*.log`) and own `TestReportSetup` teardown; logging and the MD report
  are now two fully independent mechanisms with no Allure dependency

---

## 2. Research: is CI/CD needed for this project? — TODO

**Wish (2026-10-02):** investigate whether CI/CD is needed for this project at all —
if yes, what it would look like (trigger, gates, artifacts); if no, document why not.
Research/decision first, implementation only if the answer is "yes".

**Context (already in place, no work needed to support it):**
- Test/report machinery is CI-ready: pure C#, no PowerShell dependency, no execution
  policy, all paths resolved at runtime — nothing blocks a Linux runner today
- CI only needs to archive `TestResults/` (reports) and optionally `allure-results/`
- Current gates that CI would reuse: `dotnet format --verify-no-changes`,
  `dotnet test --filter Category=HealthCheck`, full `dotnet test`

**Status history:**
- 2026-10-02 — `TODO`: added to backlog by user decision (research first, no implementation yet)
