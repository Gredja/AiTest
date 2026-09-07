---
name: commit
description: Use when the user says "commit", "/commit", or wants to commit changes. Creates a commit with safety checks (dotnet format + dotnet test) and pushes.
---

# Commit Agent for Gredja

Follow this flow precisely. Do not skip steps.

## Step 1: Status check

Run `git status` and `git diff --stat`. Show:
- Current branch
- Changed files (staged + unstaged)
- Untracked files

If working tree is clean — report and stop.

Ask for confirmation to proceed.

## Step 2: Safety gate (HARD RULE — no exceptions)

Both checks MUST pass:

```
dotnet format --verify-no-changes
```
If fails: run `dotnet format`, then re-verify.

```
dotnet test --verbosity quiet
```
If fails: show the failure, ask user to fix. Do NOT proceed with failing tests.

## Step 3: Stage

```
git add -A
```

Show staged diff summary (`git diff --cached --stat`).

## Step 4: Commit message

If the user provided a message — use it.

Otherwise, analyze staged changes and propose a message: **action + object** (e.g. "Add product API tests", "Fix model nullable properties").

Show proposed message and ask for approval.

## Step 5: Commit

```
git commit -m "<message>"
```

## Step 6: Push

If on `main` — warn and ask confirmation first.

```
git push -u origin HEAD
```

## Step 7: Report

Show:
- Branch name
- Commit hash
- Files changed
- Commit message
- Push status

---

## Rules

- Never skip safety gate
- Never commit without user approval
- Never push to `main` without explicit confirmation
- Commits: English only, format: action + object
- Never commit `.env` or tokens
