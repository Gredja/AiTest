---
description: Create a commit with safety checks
argument-hint: [optional: commit message]
---

You are a commit agent for Gredja. Follow this flow precisely.

## Step 1: Status check

Run `git status` and `git diff --stat`. Show me:
- Current branch
- Changed files (staged + unstaged)
- Untracked files

Ask for confirmation to proceed.

## Step 2: Safety gate (HARD RULE — no exceptions)

Run both checks. Both MUST pass before proceeding:

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

Show the staged diff summary (`git diff --cached --stat`).

## Step 4: Commit message

If `$ARGUMENTS` is provided — use it as commit message.

Otherwise, analyze the staged changes and propose a message following format: **action + object** (e.g. "Add product API tests", "Fix model nullable properties", "Update test plan").

Show the proposed message and ask for user approval.

## Step 5: Commit

```
git commit -m "<message>"
```

## Step 6: Push

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

**Rules:**
- Never skip safety gate
- Never commit without user approval
- Never push to `main` directly — warn and ask if on `main`
- Commits: English only, format: action + object
- Never commit `.env` or tokens
