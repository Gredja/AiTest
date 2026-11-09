---
name: review
description: Use when the user says "review", "/review", or wants a full project review. Reviews the entire Gredja codebase against all project rules. Quick single-run report (issues table + what's good), no plan file, no fixes. NOT the same as /audit (deep two-stage run with a fix plan in .mimocode/plans/ and a separate Stage 2), NOT for uncommitted changes (use /review-commit) or PR reviews (use /review-pr).
---

# Skill: Full Project Review

Review the entire Gredja project against all rules and patterns. Comprehensive audit of every .cs file.

## Step 1: Collect project files (main agent)

Run in parallel:
- `Get-ChildItem -Recurse -Include "*.cs" | Where-Object { $_.FullName -notmatch "\\obj\\" }` — list all source files
- Read `Rules/*.md` — all rule files
- Read `Core/Config/JsonPlaceholderEndpoints.cs` and `Core/Config/GitHubEndpoints.cs`

## Step 2: Spawn subagent (main agent)

Full project scan is the heaviest review operation. Delegate to subagent — main agent stays responsive.

Spawn a `general` subagent with this prompt:

```
You are a full project review subagent for Gredja. Execute the following steps precisely. Do NOT ask the user anything.

Working dir: {working_dir}

## Steps

1. Read all Rules/*.md files for project rules context.

2. Read ALL .cs files from the Step 1 glob — every source folder, never a static subset
   (Core/Attributes, Config, Generators, Helpers, Models, Logging, Reporting; Api/** tests +
   helpers incl. CoreHelpers; E2E/** tests + base classes; AllureAdapter/**; Ui/** Pages,
   Tests, Support, Helpers, moodledemo). New folders count automatically — a hardcoded
   file list goes stale and silently drops files from the review.

3. Review EACH file against ALL project rules — the single source of truth is `Rules/*.md`
   (read every rule file in full; this prompt deliberately does NOT copy rule text, to
   prevent drift between the rules and the review). Apply at minimum: code.md, code-style.md,
   code-principles.md, models.md, assertions.md, comments.md, config.md, categories.md,
   test-practices.md — plus ui-testing.md / localization-testing.md for Ui files.

4. Cross-cutting checks:
   - Dead code: unused methods, unused models, unused using statements
   - DRY violations: repeated patterns that should be extracted
   - Inconsistencies: different patterns for same operation across services
   - Missing coverage: endpoints without tests
   - Naming consistency across files
   - False-positive guards (verify BEFORE flagging): `[Ignore]`d negatives — check the documented ceiling first (`documentation/*TestPlan.md` → "Negative Floor Status", OB); unused endpoint constants — check `.mimocode/plans/` (they may be planned endpoints with ready designs)

5. Classify each issue:
   - major — must fix (rule violation, potential bug, security issue)
   - minor — should consider (style, suboptimal pattern, dead code)

6. Report:
   - Executive summary (2-3 sentences)
   - Statistics: files reviewed, issues found, major vs minor
   - Issues table: Severity | File | Line | Issue
   - Dead code inventory
   - Consistency issues across services
   - What's good (mandatory section)
```

## Step 3: Deliver result (main agent)

Report the subagent's output to the user.

---

## Rules

- Review ALL .cs files, not just a sample
- Exclude obj/ directories (auto-generated)
- Line references must be exact (file:line format)
- "What's good" section is mandatory
- If no issues found, say so explicitly
- Major issues need clear explanation of why they're blocking

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-11-08 | S8: description contrasts /review vs /audit (quick report vs two-stage plan). Revision History added | MiMo |
