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
- `Core/Config/Endpoints.cs` — if any endpoint references changed

## Step 4: Review each file against ALL project rules

- Code rules (`Rules/code.md`): PascalCase, no abbreviations, file-scoped namespaces, max ~30 lines, no magic numbers
- Model rules (`Rules/models.md`): Model/Request suffixes, property types, pure data containers
- Assertion rules (`Rules/assertions.md`): FluentAssertions only
- Comment rules (`Rules/comments.md`): default = no comments
- Config rules (`Rules/config.md`): Endpoints in Endpoints.cs, never hardcoded
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
- Always read existing similar files for pattern comparison
- Line references must be exact (file:line format)
- "What's good" section is mandatory — acknowledge good work
- If no issues found, say so explicitly — don't invent nitpicks
