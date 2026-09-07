---
name: review
description: Use when the user says "review", "/review", "review code", "review changes", or wants to review local uncommitted code changes. Reviews all modified and new files against project rules and existing patterns. NOT for PR reviews (use /review-pr instead).
---

# Skill: Code Review

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

## Step 2: Read all changed content

**Modified files** — run `git diff -- <file>` for each to see exact diffs.

**New files** — read full content of each file.

**Also read for context:**
- Existing similar files (e.g. if reviewing `UserAssertHelper`, also read `ProductAssertHelper`)
- `Rules/*.md` — all rule files in `Rules/` directory
- `Core/Config/Endpoints.cs` — if any endpoint references changed

## Step 3: Review each file

Check every changed file against ALL project rules:

### Code rules (`Rules/code.md`)
- PascalCase classes/methods/properties/constants
- camelCase locals/parameters, `_camelCase` private fields
- No abbreviations (`response` not `resp`)
- Boolean prefix: `Is`, `Has`, `Can`, `Should`
- File-scoped namespaces, one class per file
- Explicit types > var (unless obvious)
- Max ~30 lines per method, max 3-4 params
- No magic numbers/strings — extract to constants
- No nested ternaries
- `if` blocks always have `{ }`

### Model rules (`Rules/models.md`)
- Response: suffix `Model` (includes `Id`). Request: suffix `Request` (no `Id`)
- Reference types: no `?`, no initializer
- Value types: `?` only if JSON field can be null/absent
- Namespace: `Core.Models`
- Pure data containers — no constructors, validation, or logic

### Assertion rules (`Rules/assertions.md`)
- FluentAssertions only (not NUnit Assert)
- No `out _` inside `OnlyContain` lambdas
- Key patterns: `.Should().Be()`, `.NotBeNull()`, `.NotBeNullOrWhiteSpace()`, `.BeGreaterThan()`, `.BeInRange()`, `.OnlyContain()`

### Comment rules (`Rules/comments.md`)
- Default: no comments
- Exceptions: regex explanations, non-obvious WHY only

### Config rules (`Rules/config.md`)
- Endpoints in `Core/Config/Endpoints.cs`
- Never hardcode URLs in tests

### Pattern consistency
- Compare new code against existing similar code (e.g. `UserAssertHelper` vs `ProductAssertHelper`)
- Check for DRY violations — repeated setup blocks should be extracted
- Check for extension method name collisions across helpers

## Step 4: Classify and report

For each issue found, classify severity:
- **major** — must fix (rule violation, DRY violation, potential bug, naming collision)
- **minor** — should consider (style inconsistency, suboptimal pattern)
- **ok** — works correctly, no issues

### Report format

```
## Review: [feature name]

### Summary
1-2 sentence overview of what was changed and overall quality.

### Issues

| Severity | File | Line | Issue |
|----------|------|------|-------|
| major | File.cs | 12 | Description with suggestion |
| minor | File.cs | 34 | Description |

### What's good
- Bullet list of positives (mandatory section)
```

## Rules

- Review ALL changed files, not just a sample
- Always read existing similar files for pattern comparison
- Line references must be exact (file:line format)
- "What's good" section is mandatory — acknowledge good work
- If no issues found, say so explicitly — don't invent nitpicks
