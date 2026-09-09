---
name: review
description: Use when the user says "review", "/review", or wants a full project review. Reviews the entire Gredja codebase against all project rules. NOT for uncommitted changes (use /review-commit) or PR reviews (use /review-pr).
---

# Skill: Full Project Review

Review the entire Gredja project against all rules and patterns. Comprehensive audit of every .cs file.

## Step 1: Collect project files (main agent)

Run in parallel:
- `Get-ChildItem -Recurse -Include "*.cs" | Where-Object { $_.FullName -notmatch "\\obj\\" }` — list all source files
- Read `Rules/*.md` — all rule files
- Read `Core/Config/FakeStoreEndpoints.cs` and `Core/Config/JsonPlaceholderEndpoints.cs`

## Step 2: Spawn subagent (main agent)

Full project scan is the heaviest review operation. Delegate to subagent — main agent stays responsive.

Spawn a `general` subagent with this prompt:

```
You are a full project review subagent for Gredja. Execute the following steps precisely. Do NOT ask the user anything.

Working dir: {working_dir}

## Steps

1. Read all Rules/*.md files for project rules context.

2. Read ALL .cs files (excluding obj/ directories):
   - Core/Attributes/*.cs
   - Core/Config/*.cs
   - Core/Helpers/*.cs
   - Core/Models/**/*.cs
   - Api/FakeStore/Tests/*.cs
   - Api/JsonPlaceholder/Tests/*.cs
   - Api/AllureGlobalSetup.cs
   - TestAdapter/Helpers/*.cs
   - Ui/Tests/*.cs
   - Ui/AllureGlobalSetup.cs

3. Review EACH file against ALL project rules:

   Code rules (Rules/code.md):
   - PascalCase for classes, methods, properties, constants
   - camelCase for locals, params
   - _camelCase for private fields
   - No abbreviations (response, not resp)
   - Boolean prefixes: Is, Has, Can, Should
   - File-scoped namespaces
   - One class per file
   - Explicit types > var (unless obvious)
   - Methods: short, one responsibility, max ~30 lines, max 5 params
   - All API requests async
   - No magic numbers
   - No nested ternary
   - nameof() for exceptions
   - {} for all if blocks, even single-line

   Model rules (Rules/models.md):
   - Response: suffix Model (includes Id)
   - Request: suffix Request (no Id)
   - Reference types: no ?, no initializer
   - Value types: ? only if JSON field can be null/absent
   - Namespace: Core.Models
   - Pure data containers — no constructors, validation, logic

   Assertion rules (Rules/assertions.md):
   - FluentAssertions only (no NUnit Assert)
   - Key patterns: .Should().Be(), .NotBeNull(), .NotBeNullOrWhiteSpace(), .BeGreaterThan(), .BeInRange(), .OnlyContain()

   Comment rules (Rules/comments.md):
   - Default: no comments
   - Exceptions: regex explanations, TODO (remove before merge), non-obvious WHY

   Config rules (Rules/config.md):
   - Base URL and endpoints in FakeStoreEndpoints.cs/JsonPlaceholderEndpoints.cs
   - Never hardcode in tests

   Category rules (Rules/categories.md):
   - [Category] on [TestFixture] class — service type
   - [Category] on [Test] method — check type
   - Both dimensions applied

4. Cross-cutting checks:
   - Dead code: unused methods, unused models, unused using statements
   - DRY violations: repeated patterns that should be extracted
   - Inconsistencies: different patterns for same operation across services
   - Missing coverage: endpoints without tests
   - Naming consistency across files

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
