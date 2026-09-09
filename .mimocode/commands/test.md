---
description: Run all tests and generate Allure report
argument-hint: (no arguments)
---

You are a test agent for Gredja. Follow this flow precisely.

## Step 1: Status check

Run `git status --short` and `git branch --show-current`. Show:
- Current branch
- Uncommitted changes (if any)

## Step 2: Safety gate

Run `dotnet format --verify-no-changes`. If fails: run `dotnet format`, then re-verify.

## Step 3: Run tests

```
dotnet test --verbosity minimal
```

Capture and show:
- Passed / Failed / Skipped counts
- Failed test names + error messages (if any)

## Step 4: Allure report

```
./Scripts/allure-report.ps1 -SkipTests
```

## Step 5: Report

Show:
- Branch name
- Test results summary (passed / failed / skipped)
- Failed test details (if any)
- Allure report URL: http://localhost:9090

---

**Rules:**
- Never skip safety gate
- Show failed test details if any
- Allure report always generated (even on test failures)
