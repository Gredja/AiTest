---
description: Review local uncommitted code changes against project rules
argument-hint: (no arguments)
---

# Code Review — Commit

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

## Step 2: Spawn subagent

Spawn a `general` subagent with this prompt:

```
You are a code review subagent for Gredja. Execute the following steps precisely. Do NOT ask the user anything.

Working dir: {working_dir}

Changed files:
- Modified: {modified_files}
- New: {new_files}
- Deleted: {deleted_files}

## Steps

1. Read changed content:
   - For modified files: run `git diff -- <file>` for each to see exact diffs
   - For new files: read full content of each file

2. Read context:
   - Existing similar files (e.g. if reviewing UserAssertHelper, also read ProductAssertHelper)
   - Rules/*.md — all rule files in Rules/ directory
   - Core/Config/FakeStoreEndpoints.cs and Core/Config/JsonPlaceholderEndpoints.cs — if any endpoint references changed

3. Review each file against ALL project rules:
   - Code rules (Rules/code.md): PascalCase, no abbreviations, file-scoped namespaces, max ~30 lines, no magic numbers
   - Model rules (Rules/models.md): Model/Request suffixes, property types, pure data containers
   - Assertion rules (Rules/assertions.md): FluentAssertions only
   - Comment rules (Rules/comments.md): default = no comments
   - Config rules (Rules/config.md): Endpoints in FakeStoreEndpoints.cs/JsonPlaceholderEndpoints.cs, never hardcoded
   - Pattern consistency: compare against existing similar code, check for DRY violations

4. Classify each issue:
   - major — must fix (rule violation, potential bug)
   - minor — should consider (style, suboptimal pattern)

5. Report:
   - Summary (1-2 sentences)
   - Issues table: Severity | File | Line | Issue
   - What's good (mandatory section)
```

## Step 3: Deliver result

Report the subagent's output to the user.

---

## Rules

- Review ALL changed files, not just a sample
- Always read existing similar files for pattern comparison
- Line references must be exact (file:line format)
- "What's good" section is mandatory — acknowledge good work
- If no issues found, say so explicitly — don't invent nitpicks
