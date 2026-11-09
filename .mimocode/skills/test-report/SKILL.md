---
name: test-report
description: Use when the user says "test-report", "/test-report", "allure", or wants to run all tests and see the Allure report. Runs ALL tests and generates Allure report.
---

# Test Report Agent for Gredja

Orchestrate test run via one subagent, generate Allure report from main agent.

## Flow

```
main agent
  ├── clean allure-results/
  ├── spawn subagent (test runner)
  │     ├── dotnet format --verify-no-changes
  │     ├── & ./Scripts/test-parallel-rerun.ps1   (two-pass: 8 workers → failed rerun in 1)
  │     └── send results → main agent
  ├── generate Allure report (main agent)
  └── report to user
```

Note: Allure runs from main agent because subagents cannot get user approval for bash permission prompts.

## Step 1: Quick pre-check (main agent)

Run `dotnet test --verbosity minimal --filter Category=HealthCheck` and show:
- Current branch
- Quick pass/fail summary
- Report file: the newest `TestResults/TestRunReport-*.md` (written automatically after every run; path is not printed to console)

## Step 2: Clean old results (main agent)

Run `Remove-Item "allure-results\*" -Force -ErrorAction SilentlyContinue` in working dir.

MANDATORY — prevents accumulated test counts in Allure.

## Step 3: Spawn test subagent (main agent)

Spawn `general` subagent in BACKGROUND with this prompt:

```
You are a test subagent for Gredja. Execute the following steps precisely. Do NOT ask the user anything.

Working dir: {working_dir}

## Steps

1. Safety gate:
   - Run `dotnet format --verify-no-changes`. If fails: run `dotnet format`, then re-verify.

2. Run all tests (two-pass: pass 1 in 8 workers, failed tests rerun sequentially in 1 worker):
   - Run `& ./Scripts/test-parallel-rerun.ps1` (from a PowerShell session — same script as /test and the nightly workflow)
   - Capture the verdict from the exit code: exit 0 = green (either pass 1 green or all
     failures recovered on the sequential rerun — those are flaky, listed with `~`);
     exit 1 = persistent failures (listed with `x`)
   - Report files: every pass writes `TestResults/TestRunReport-*.md` — with failures the
     NEWEST file is the pass-2 verdict, pass 1 holds the full per-test table
   - Do NOT clean `allure-results` between passes: the adapter writes a stable `testCaseId`
     per test, so Allure renders a pass-1 failure + pass-2 recovery as a RETRY of one test —
     cleaning or regenerating would destroy that signal

3. Report to parent:
   - Branch name
   - Test results: passed / failed / skipped counts
   - Failed test details (name + error) if any
   - Report file path (newest `TestResults/TestRunReport-*.md`)
```

Wait for test subagent to complete. Collect test results from its output.

## Step 4: Generate Allure report (main agent)

Once test results received, run directly from main agent:

```
./Scripts/allure-report.ps1 -SkipTests
```

Wait for completion. This generates the report and starts the Allure server on port 9090.

## Step 5: Report to user (main agent)

Combine outputs:
- Branch name
- Test results: passed / failed / skipped counts
- Failed test details (name + error) if any
- Run `/coverage` skill for coverage data
- Allure report URL: http://localhost:9090

---

## Rules

- Never skip safety gate
- Show failed test details if any — split into persistent (`x`, exit 1) vs recovered flaky (`~`)
- Two-pass runner (`test-parallel-rerun.ps1`) — same as /test and nightly; never downgrade to a plain single `dotnet test`
- `allure-results` are kept across both passes on purpose (stable `testCaseId` → Allure retries)
- Allure report always generated (even on test failures)
- One subagent: test runner only
- Allure generation runs from main agent (not subagent)
- Coverage via `/coverage` skill (separate subagent)

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-11-08 | Subagent runner switched to two-pass test-parallel-rerun.ps1 (flaky triage parity with /test and nightly); allure-results kept across passes for retry rendering; Rules + Revision History (S4) | MiMo |
