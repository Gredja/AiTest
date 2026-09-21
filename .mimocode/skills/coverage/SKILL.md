---
name: coverage
description: Use when the user says "coverage", "/coverage", or wants to see test coverage report. Runs code coverage (coverlet) + file coverage (endpoint→test mapping).
---

# Coverage Agent for Gredja

Calculate and report test coverage: code coverage (lines/branches via coverlet) + file coverage (endpoint→test mapping).

## Step 1: Confirm with user (main agent)

Run `./Scripts/test-coverage.ps1` as a quick pre-check. Show:
- Current branch
- Quick coverage summary

Ask for confirmation to proceed.

## Step 2: Spawn subagent (main agent)

Once user confirmed, spawn a `general` subagent with this prompt:

```
You are a coverage subagent for Gredja. Execute the following steps precisely. Do NOT ask the user anything.

Working dir: {working_dir}

## Steps

1. Safety gate:
   - Run `dotnet format --verify-no-changes`. If fails: run `dotnet format`, then re-verify.

2. Run coverage:
   - Run `./Scripts/test-coverage.ps1`
   - Capture output: lines/branches coverage + file coverage table

3. Report:
   - Branch name
   - Code coverage: lines % + branches % (per dll)
   - File coverage per service:
     ```
     | Service | Endpoints | Tested | Coverage |
     |---------|-----------|--------|----------|
     | FakeStore | 8 | 4 | 50% |
     | ... | ... | ... | ... |
     ```
   - List of untested endpoints per service
   - HTML report path: TestResults/Report/index.html
```

## Step 3: Deliver result (main agent)

Report the subagent's output to the user.

---

## Rules

- Always run safety gate first
- HTML report opens automatically in browser after generation
- File coverage counts test FILES, not individual test methods
- Exclude non-endpoint constants from total count
- Report both code coverage and file coverage — never just one
