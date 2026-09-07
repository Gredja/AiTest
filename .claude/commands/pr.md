---
description: Create a PR with safety checks
argument-hint: [optional: branch name]
---

You are a PR agent for Gredja. Follow this flow precisely.

## Step 1: Status check

Run `git status` and `git diff --stat`. Show me:
- Current branch
- Changed files (staged + unstaged)
- Untracked files

Ask for confirmation to proceed.

## Step 2: Branch

If `$ARGUMENTS` is provided, use it as branch name (prefix with `features/`).
Otherwise, suggest a branch name based on the changes (e.g. `features/add-product-tests`).

If already on a feature branch — confirm. If on `main` — create and switch.

```
git checkout -b features/<name>
```

## Step 3: Safety gate (HARD RULE — no exceptions)

Run both checks. Both MUST pass before proceeding:

```
dotnet format --verify-no-changes
```
If fails: run `dotnet format`, then re-verify.

```
dotnet test --verbosity quiet
```
If fails: show the failure, ask user to fix. Do NOT proceed with failing tests.

## Step 4: Commit

Stage all changes: `git add -A`

Ask user for commit message, or propose one following format: **action + object** (e.g. "Add product API tests", "Fix model nullable properties").

```
git commit -m "<message>"
```

## Step 5: Push

```
git push -u origin features/<name>
```

## Step 6: Create PR

Use `gh pr create` with a clear body:

```bash
gh pr create --title "<title>" --body "## Summary
- <what changed>
- <why>

## Verification
- [x] dotnet format passed
- [x] dotnet test passed"
```

## Step 7: Report

Show:
- PR URL
- Branch name
- Files changed
- Next steps (review, merge)

---

**Rules:**
- Never skip safety gate
- Never commit without user approval
- Never push to `main` directly
- If `gh` is not installed — fall back to printing the PR URL manually for GitHub web creation
