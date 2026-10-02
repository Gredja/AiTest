---
name: test
description: Use when the user says "test", "/test", or wants to run all tests. Runs ALL tests (no category filter, no Allure report). Fast check.
---

# Test Agent for Gredja

Run **all tests** (every category, every service) without Allure report.

## Step 1: Confirm with user (main agent)

Run `dotnet test --verbosity minimal --filter Category=HealthCheck` as a quick pre-check. Show:
- Current branch
- Quick pass/fail summary
- Report file: the newest `TestResults/TestRunReport-*.md` (written automatically after every run; path is not printed to console)

Ask for confirmation to proceed.

## Step 2: Spawn subagent (main agent)

Once user confirmed, spawn a `general` subagent with this prompt:

```
You are a test subagent for Gredja. Execute the following steps precisely. Do NOT ask the user anything.

Working dir: {working_dir}

## Steps

1. Safety gate:
   - Run `dotnet format --verify-no-changes`. If fails: run `dotnet format`, then re-verify.

2. Run all tests:
   - Run `dotnet test --verbosity minimal`
   - Capture output: passed/failed/skipped counts
   - Report file: the newest `TestResults/TestRunReport-*.md` (contains the same counts + per-test table + failed-test action traces)

3. Run coverage:
   - Run `./Scripts/test-coverage.ps1`
   - Capture output: lines/branches coverage + file coverage table

4. Report:
   - Branch name
   - Test results: passed / failed / skipped counts
   - Failed test details (name + error) if any
```

## Step 3: Deliver result (main agent)

Report the subagent's output to the user (including the newest `TestResults/TestRunReport-*.md` path), then run `/coverage` skill for coverage data.

---

## Rules

- Never skip safety gate
- Show failed test details if any
- No Allure report — use /test-report if Allure is needed
- Every `dotnet test` run automatically produces one report file in `TestResults/`
  (NUnit teardown generator + run-start marker) — no extra flags needed
- The report includes a `## Failed tests` section with full error, stack trace and the
  HTTP action trace of each failed test; raw run logs (actions, per-test results) live
  in `%TEMP%\GredjaTestRun` — `TestResults/` holds only report files
