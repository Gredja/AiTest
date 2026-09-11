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
  │     ├── dotnet test
  │     └── send results → main agent
  ├── generate Allure report (main agent)
  └── report to user
```

Note: Allure runs from main agent because subagents cannot get user approval for bash permission prompts.

## Step 1: Quick pre-check (main agent)

Run `dotnet test --verbosity minimal` and show:
- Current branch
- Quick pass/fail summary

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

2. Run all tests:
   - Run `dotnet test --verbosity minimal`
   - Capture output: passed / failed / skipped counts

3. Report to parent:
   - Branch name
   - Test results: passed / failed / skipped counts
   - Failed test details (name + error) if any
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
- Allure report URL: http://localhost:9090

---

## Rules

- Never skip safety gate
- Show failed test details if any
- Allure report always generated (even on test failures)
- One subagent: test runner only
- Allure generation runs from main agent (not subagent)
