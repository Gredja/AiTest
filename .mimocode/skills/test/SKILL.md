---
name: test
description: Use when the user says "test", "/test", or wants to run all tests. Runs ALL tests (no category filter, no Allure report). Fast check.
---

# Test Agent for Gredja

Run **all tests** (every category, every service) without Allure report.

## Step 1: Confirm with user (main agent)

Run `dotnet test --verbosity minimal` as a quick pre-check. Show:
- Current branch
- Quick pass/fail summary

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

3. Report:
   - Branch name
   - Test results: passed / failed / skipped counts
   - Failed test details (name + error) if any
```

## Step 3: Deliver result (main agent)

Report the subagent's output to the user.

---

## Rules

- Never skip safety gate
- Show failed test details if any
- No Allure report — use /test-report if Allure is needed
