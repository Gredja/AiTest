---
name: audit
description: Use when the user says "audit", "/audit", "аудит", or wants to audit and fix code against project rules. Audits the entire codebase against Rules/*.md, presents findings by category, then fixes approved items. NOT for uncommitted changes review (use /review-commit) or PR reviews (use /review-pr). Unlike /review (read-only), /audit also applies fixes.
---

# Skill: Full Code Audit

Audit the entire Gredja codebase against all project rules, present findings, and fix approved items.

## Step 1: Load rules (main agent)

Read all files in `Rules/` directory. These are the authoritative rules for the audit.

## Step 2: Collect files (main agent)

List all .cs source files:
```powershell
Get-ChildItem -Recurse -Include "*.cs" | Where-Object { $_.FullName -notmatch "\\obj\\" } | Select-Object -ExpandProperty FullName
```

## Step 3: Audit by category (main agent + subagents)

Run audits in **parallel** using `explore` subagents. Group files by audit category:

### Category A: Naming & Types (explore subagent)
Check `Rules/code.md` rules:
- PascalCase for classes, methods, properties, constants
- camelCase for locals, parameters
- `_camelCase` for private fields
- No abbreviations in names
- Boolean prefix: Is, Has, Can, Should
- File-scoped namespaces, one class per file
- `var` usage

### Category B: Code Style (explore subagent)
Check `Rules/code-style.md` rules:
- Error handling (specific exceptions, no exception flow control)
- LINQ (Any over Count, no unnecessary ToList)
- Strings (interpolation, StringBuilder in loops)
- Magic strings (3+ occurrences → const)
- Null safety (`is not null`, no `null!`, `?.` and `??`)
- Pattern matching (switch expressions, `is` type pattern)

### Category C: Code Principles (explore subagent)
Check `Rules/code-principles.md` rules:
- Guard clauses (early return to flatten nesting)
- Expression-bodied members (=> for single-expression)
- No magic numbers
- No nested ternaries
- Braces for all if blocks
- Empty line before return

### Category D: Models & Config (explore subagent)
Check `Rules/models.md` and `Rules/config.md`:
- Model naming (Model suffix, Request suffix)
- Property types (reference vs value nullable rules)
- No constructors/validation in models
- Endpoints in config files, not hardcoded
- `[JsonPropertyName]` for ambiguous fields

### Category E: Tests & Assertions (explore subagent)
Check `Rules/assertions.md`, `Rules/test-practices.md`, `Rules/categories.md`:
- FluentAssertions only (no NUnit Assert)
- Test isolation (independent tests)
- Given/When/Then structure
- Separate status/body assertions
- Category attributes on classes and methods

### Category F: Comments (explore subagent)
Check `Rules/comments.md`:
- No unnecessary comments
- Regex explanations present
- No TODO/FIXME in committed code

Each subagent reports findings as a table:
```
| Severity | File | Line | Rule | Current | Suggested |
```

Severity: `major` (rule violation, bug risk) | `minor` (style, suboptimal) | `style` (cosmetic)

## Step 4: Present report (main agent)

Combine all subagent results into a unified report:

```
# Audit Report

**Files reviewed:** N
**Findings:** X major, Y minor, Z style

## Major (must fix)
| # | File | Line | Rule | Issue | Fix |
|---|------|------|------|-------|-----|

## Minor (should fix)
| # | File | Line | Rule | Issue | Fix |
|---|------|------|------|-------|-----|

## Style (optional)
| # | File | Line | Rule | Issue | Fix |
|---|------|------|------|-------|-----|
```

Ask the user: "Какие пункты исправлять?" (all / major only / specific numbers)

## Step 5: Apply fixes (main agent)

For each approved finding:
1. Read the file
2. Apply the fix
3. Track what was changed

Group fixes by file to minimize reads/edits.

After ALL fixes in a category are applied:
```bash
dotnet build --no-restore -v q 2>&1 | Select-String "Error|Build succeeded"
```

If build fails — revert the last fix and report the issue.

After build succeeds:
```bash
dotnet test --no-build --verbosity q
```

If tests fail — investigate, fix or revert, re-run tests.

## Step 6: Report results (main agent)

After all fixes are applied and tests pass:

```
# Audit Complete

| Category | Fixed |
|----------|-------|
| Naming & Types | N |
| Code Style | N |
| ... | ... |

**Tests:** X passed, Y failed
**Build:** Clean (0 errors)
```

Update the plan file if one exists, or note findings for future sessions.

---

## Rules

- Audit ALL .cs files, not a sample
- Line references must be exact (`file:line`)
- Never fix without user approval — present first, fix after
- Run `dotnet build` after each category of fixes
- Run `dotnet test` after all fixes are complete
- If a fix breaks the build — revert immediately and report
- Group related fixes (e.g. all null safety together) for efficient application
- Skip auto-generated code (obj/, bin/)
- Exclude `AllureTestResultBuilder` dictionary keys from magic strings audit (Allure JSON spec)
- Category attributes (`[Category("HealthCheck")]`) are enum-like names, not magic strings
- Fallback values (`"Tests"`, `"Unknown"`) stay inline
