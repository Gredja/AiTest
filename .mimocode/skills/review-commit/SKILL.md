---
name: review-commit
description: Use when the user says "review-commit", "/review-commit", "review code", "review changes", or wants to review local uncommitted code changes. Reviews all modified and new files against project rules and existing patterns. NOT for PR reviews (use /review-pr instead).
---

# Skill: Code Review — Commit

All steps executed directly by the main agent. No subagent.

Review all local uncommitted changes (modified + new/untracked files) against Gredja project rules and existing code patterns.

## Step 1: Identify changed files

Run in parallel:
- `git status` — see all modified, deleted, untracked files
- `git diff --stat HEAD` — summary of changes vs last commit

If working tree is clean — report "No uncommitted changes" and stop.

Collect three lists:
1. **Modified files** (tracked, changed)
2. **New files** (untracked)
3. **Deleted files**

## Step 2: Read changed content

- For modified files: run `git diff -- <file>` for each to see exact diffs
- For new files: read full content of each file

## Step 3: Read context

- Existing similar files (e.g. if reviewing UserAssertHelper, also read ProductAssertHelper)
- `Rules/*.md` — all rule files in Rules/ directory
- `Core/Config/JsonPlaceholderEndpoints.cs` and `Core/Config/GitHubEndpoints.cs` — if any endpoint references changed

## Step 4: Review each file against ALL project rules

Rules are read from `Rules/*.md` in Step 3 — this skill does NOT restate their text (a
restatement drifts the moment a rule file is edited; single source of truth, same
principle as /review and /commit). Match each changed file to the rules by subject and
re-open the Rules file whenever wording decides a finding:

| File kind | Rules to apply |
|---|---|
| any `.cs` | `code.md`, `code-style.md`, `code-principles.md`, `comments.md` |
| models / config | `models.md`, `config.md` |
| tests | `assertions.md`, `test-practices.md`, `categories.md` |
| `Ui/**`, moodledemo | additionally `ui-testing.md`, `localization-testing.md` |
| every file | `git.md` (secrets/branch hygiene), `workflow.md` |

Review-only heuristics (NOT rule text — keep as-is):
- False-positive guards (verify BEFORE flagging): `[Ignore]`d negatives — check the documented ceiling first (`documentation/*TestPlan.md` → "Negative Floor Status", OB); unused endpoint constants — check `.mimocode/plans/` (they may be planned endpoints with ready designs)
- Pattern consistency: compare against existing similar code, check for DRY violations

## Step 5: Classify each issue

- **major** — must fix (rule violation, potential bug)
- **minor** — should consider (style, suboptimal pattern)

## Step 6: Report

- Summary (1-2 sentences)
- Issues table: Severity | File | Line | Issue
- What's good (mandatory section)

---

## Rules

- Review ALL changed files, not just a sample
- Review against `Rules/*.md` files read in Step 3 — never against a restated checklist (single source of truth)
- Always read existing similar files for pattern comparison
- Line references must be exact (file:line format)
- "What's good" section is mandatory — acknowledge good work
- If no issues found, say so explicitly — don't invent nitpicks

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-11-08 | Step 4: restated rule bullets replaced with subject→file mapping table (Rules stay the single source of truth; false-positive guards kept as review heuristics) (S6) + Revision History added | MiMo |
