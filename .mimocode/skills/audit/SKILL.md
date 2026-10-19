---
name: audit
description: Use when the user says "audit", "/audit", "аудит", or wants to audit and fix code against project rules. Audits the entire codebase against Rules/*.md, presents findings by category, then applies approved fixes in gated batches (build/format/tests after each batch, one /commit per batch) — the audit includes the refactoring. NOT for uncommitted changes review (use /review-commit) or PR reviews (use /review-pr). Unlike /review (read-only), /audit also applies fixes.
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
- Magic strings (2+ occurrences of same meaning → const — see Rules/code-style.md)
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

If the user wants a trackable order (or grants autonomy up front), write the findings first to
`.mimocode/plans/audit-fixes-<YYYY-MM-DD>.md`: numbered items grouped into fix batches, my chosen
fix order, and a SKIP section with reasons for items deliberately not fixed — that file is the
worklist for Step 5 and is deleted when all batches close.

## Step 5: Apply fixes with refactor discipline (main agent)

Fixing IS refactoring — apply the same hard discipline as the `/refactor` skill:

1. **Batch the fixes** by area/phase (comments → Core → Api → E2E → Ui → adapters), not one
   category at a time: a batch is one commit-worth of coherent change. Order batches from
   independent/safe to broad (moves first if a fix requires structural relocation).
2. Per batch: read the target files, apply the adjudicated fixes, track each id from the plan file.
3. **Gates after EVERY batch** (never skip):
   ```bash
   dotnet build --verbosity quiet
   dotnet format --verify-no-changes
   dotnet test --verbosity minimal --filter Category=HealthCheck
   # plus the suites the batch touched, e.g.:
   #   Api/GitHub code   → --filter "Category=GitHub&Category!=GitHubUi&Category!=GitHubE2E"
   #   Ui code           → --filter Category=GitHubUi
   #   E2E code          → --filter Category=GitHubE2E
   ```
   If build/format fails — fix or revert the batch's last change and re-gate.
   If a touched suite goes red — investigate, fix or revert; never commit a red batch.
4. **Commit per batch ONLY through the `/commit` skill** (message approval, all-rules review,
   backups/education sync, `normalize-eol`, push). Never `git commit` directly.
5. Structural moves found during fixes (file/folder renames, namespace changes) follow the
   `/refactor` Phase-R rule: one move per flow + docs sync (`FILE_STRUCTURE.md`, `AGENTS.md`,
   `Rules/*`, `backups/SETUP.md`) + gates, still committed via `/commit`.
6. Items that should NOT be fixed go to the plan's SKIP section with a concrete reason
   (accepted debt, rule scope nuance, churn worse than the smell) — surfaced to the user in
   the final report, never silently dropped.

If tests fail — investigate, fix or revert, re-run the batch gates.

## Step 6: Report results (main agent)

After all batches are applied, gated and committed:

```
# Audit Complete

| Category | Fixed | Skipped (reason) |
|----------|-------|------------------|
| Naming & Types | N | ... |
| ... | | |

**Gates:** build clean · format pass · HealthCheck X/X · touched suites: <numbers>
**Commits:** <batch hashes>
**Plan file:** deleted (all batches closed) / open items: ...
```

Optional verify pass: re-run one quick audit category over the changed files only, to confirm
the fixes did not introduce new violations.

---

## Rules

- Audit ALL .cs files, not a sample
- Line references must be exact (`file:line`)
- Never fix without user approval — present first, fix after (an upfront autonomy grant or an
  approved plan file counts as that approval for its listed items)
- Gates after EVERY fix batch: `build` + `format --verify-no-changes` + `HealthCheck` + touched suites
- Commit per batch ONLY through the `/commit` skill — never `git commit` directly
- If a fix breaks the build — revert immediately and report
- Group related fixes (e.g. all null safety together) for efficient application
- SKIP items need an explicit reason in the plan file / final report — no silent drops
- Skip auto-generated code (obj/, bin/)
- Exclude `AllureTestResultBuilder` dictionary keys from magic strings audit (Allure JSON spec)
- Category attributes (`[Category("HealthCheck")]`) are enum-like names, not magic strings
- Fallback values (`"Tests"`, `"Unknown"`) stay inline
