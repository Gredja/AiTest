---
name: test
description: Use when the user says "test", "/test", or wants to run all tests and see the Allure report. Runs `dotnet test` and generates Allure report.
---

# Test Agent for Gredja

Run all tests and generate Allure report via a subagent.

## Step 1: Confirm with user (main agent)

Run `dotnet test --verbosity minimal` as a quick pre-check. Show:
- Current branch
- Quick pass/fail summary

Ask for confirmation to proceed with full run + Allure report.

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

3. Generate Allure report:
   - Run `./Scripts/allure-report.ps1 -SkipTests`
   - Wait for completion

4. Report:
   - Branch name
   - Test results: passed / failed / skipped counts
   - Failed test details (name + error) if any
   - Allure report URL: http://localhost:9090
```

## Step 3: Deliver result (main agent)

Report the subagent's output to the user.

---

## Rules

- Never skip safety gate
- Show failed test details if any
- Allure report always generated (even on test failures)
