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

2. Run all tests (two-pass: pass 1 in 8 workers, failed tests rerun sequentially in 1 worker):
   - Run `& ./Scripts/test-parallel-rerun.ps1` (from a PowerShell session — the nightly workflow runs the same script via `shell: pwsh`)
   - Capture the verdict: exit 0 = green (either pass 1 green or all failures recovered on the
     sequential rerun — those are listed with `~` as flaky); exit 1 = persistent failures
     (listed with `x`)
   - Report files: every pass writes `TestResults/TestRunReport-*.md` — with failures the
     NEWEST file is the pass-2 verdict, pass 1 holds the full per-test table and traces

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
- Show failed test details if any — split into persistent (`x`, exit 1) vs recovered flaky (`~`)
- No Allure report — use /test-report if Allure is needed
- Every `dotnet test` run automatically produces one report file in `TestResults/`
  (NUnit teardown generator + run-start marker) — no extra flags needed; a two-pass run
  therefore writes TWO files (pass 1 + pass 2)
- The report includes a `## Failed tests` section with full error, stack trace and the
  HTTP action trace of each failed test; raw run logs (actions, per-test results) live
  in `%TEMP%\GredjaTestRun` — `TestResults/` holds only report files
