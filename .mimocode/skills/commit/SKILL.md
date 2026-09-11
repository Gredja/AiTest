---
name: commit
description: Use when the user says "commit", "/commit", or wants to commit changes. Creates a commit with safety checks (format + HealthCheck tests) and pushes. If HealthCheck tests fail, commit is blocked.
---

# Commit — Gredja

All steps executed directly by the main agent. No subagent.

## Step 1: Status check

Run `git status` and `git diff --stat`. Show:
- Current branch
- Changed files (staged + unstaged)
- Untracked files

If working tree is clean — report and stop.

## Step 2: Gather commit message

If the user provided a commit message — use it.

Otherwise, analyze the staged changes and propose a message: **action + object** (e.g. "Add product API tests", "Fix model nullable properties").

Show proposed message and ask for approval.

## Step 3: Safety gate — format check

```
dotnet format --verify-no-changes
```

If fails: run `dotnet format`, then re-verify. If still fails — report and STOP.

## Step 4: Code review

Run `git diff` to see all unstaged changes.

Read `Rules/code.md`, `Rules/code-style.md`, `Rules/code-principles.md`, `Rules/models.md`, `Rules/assertions.md`, `Rules/test-practices.md`, `Rules/comments.md` for full project rules.

Review each changed file against ALL project rules:
- Naming, types, file layout, methods, async, general (from code.md)
- Models: suffixes, properties, no constructors/logic (from models.md)
- Assertions: FluentAssertions only (from assertions.md)
- Comments: default = no comments (from comments.md)
- No magic numbers — extract to constants
- Config: Endpoints in `Core/Config/FakeStoreEndpoints.cs` and `Core/Config/JsonPlaceholderEndpoints.cs`, never hardcoded in tests

If blocking issues found: report them and STOP. Do not commit.

If only suggestions/nits: report them but proceed.

## Step 5: HealthCheck tests

```
dotnet test --verbosity minimal --filter Category=HealthCheck
```

If ANY tests fail: report the failures and STOP. Do not commit.

## Step 6: Sync backups

Check if any of the following were changed in this commit:
- `.mimocode/mimocode.jsonc` → update `backups/mimocode-project.jsonc`
- `Rules/*.md` or `Prompts/templates/*` affecting test generation → update `backups/Prompts-templates/`
- Anything affecting new user setup (permissions, tools, env vars) → update `backups/SETUP.md`
- Structural changes (new files, moved files, deleted files) → update `documentation/FILE_STRUCTURE.md`

If nothing changed — skip. If something changed — update the corresponding backup file.

## Step 7: Stage, commit, push

```
git add -A
git diff --cached --stat
git commit -m "{message}"
```

If branch is `main` — do NOT push. Report that push was skipped.
Otherwise: `git push -u origin HEAD`

## Step 8: Report

- Branch name
- Commit hash (from `git log -1 --format="%H"`)
- Files changed
- Commit message
- Push status (pushed / skipped)
- Backup sync status (synced / skipped with reason)
- Review findings (if any)

---

## Rules

- Never skip safety gate (format check + HealthCheck tests)
- HealthCheck tests must pass — if any fail, STOP. Do not commit.
- Never skip code review
- Never commit without user approval (gathered in Step 1-2)
- Never push to `main` without explicit confirmation
- Commits: English only, format: action + object
- Never commit `.env` or tokens
- Always sync backups/ before commit if relevant files changed
