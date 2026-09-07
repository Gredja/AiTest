---
name: commit
description: Use when the user says "commit", "/commit", or wants to commit changes. Creates a commit with safety checks (dotnet format + dotnet test) and pushes.
---

# Commit Agent for Gredja

Orchestrate a commit via a subagent. The main agent gathers inputs; the subagent executes.

## Step 1: Status check (main agent)

Run `git status` and `git diff --stat`. Show:
- Current branch
- Changed files (staged + unstaged)
- Untracked files

If working tree is clean — report and stop.

Ask for confirmation to proceed.

## Step 2: Gather commit message (main agent)

If the user provided a commit message — use it.

Otherwise, analyze the staged changes and propose a message: **action + object** (e.g. "Add product API tests", "Fix model nullable properties").

Show proposed message and ask for approval.

## Step 3: Spawn subagent (main agent)

Once user confirmed AND commit message approved, spawn a `general` subagent with this prompt:

```
You are a commit subagent for Gredja. Execute the following steps precisely. Do NOT ask the user anything — all inputs are provided below.

Branch: {branch}
Commit message: {message}
Working dir: {working_dir}

## Steps

1. Safety gate (HARD RULE — no exceptions):
   - Run `dotnet format --verify-no-changes`. If fails: run `dotnet format`, then re-verify.
   - Run `dotnet test --verbosity quiet`. If fails: report the failure and STOP. Do not commit.

2. Code review (before staging):
   - Run `git diff` to see all unstaged changes
   - Read `Rules/code.md`, `Rules/models.md`, `Rules/comments.md`, `Rules/assertions.md` for full project rules
   - Review each changed file against ALL project rules, including:
     * Naming, types, file layout, methods, async, general (from code.md)
     * Models: suffixes, properties, no constructors/logic (from models.md)
     * Assertions: FluentAssertions only (from assertions.md)
     * Comments: default = no comments (from comments.md)
     * No magic numbers — extract to constants
     * Config: Endpoints in `Core/Config/Endpoints.cs`, never hardcoded in tests
   - If blocking issues found: report them and STOP. Do not commit.
   - If only suggestions/nits: report them but proceed with commit.

3. Stage:
   - Run `git add -A`
   - Run `git diff --cached --stat` to confirm

4. Commit:
   - Run `git commit -m "{message}"`

5. Push:
   - If branch is `main` — do NOT push. Report that push was skipped.
   - Otherwise: run `git push -u origin HEAD`

6. Report:
   - Branch name
   - Commit hash (from git log -1 --format="%H")
   - Files changed
   - Commit message
   - Push status (pushed / skipped)
   - Review findings (if any)
```

## Step 4: Deliver result (main agent)

Report the subagent's output to the user.

---

## Rules

- Never skip safety gate
- Never skip code review
- Never commit without user approval (gathered in Step 1-2 before spawning)
- Never push to `main` without explicit confirmation
- Commits: English only, format: action + object
- Never commit `.env` or tokens
