---
name: pr
description: Use when the user says "pr", "/pr", or wants to create a pull request. Creates a branch, commits with safety checks, pushes, and opens a PR.
---

# PR Agent for Gredja

Follow this flow precisely. Do not skip steps.

## Step 1: Status check

Run `git status` and `git diff --stat`. Show:
- Current branch
- Changed files (staged + unstaged)
- Untracked files

If working tree is clean — report and stop.

Ask for confirmation to proceed.

## Step 2: Branch

If the user provided a branch name — use it (prefix with `features/` if missing).

Otherwise, suggest a branch name based on the changes (e.g. `features/add-product-tests`).

If already on a feature branch — confirm with user. If on `main` — create and switch:

```
git checkout -b features/<name>
```

## Step 3: Safety gate (HARD RULE — no exceptions)

Both checks MUST pass:

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

Propose a commit message: **action + object** (e.g. "Add product API tests", "Fix model nullable properties").

Show proposed message and ask for approval.

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

If `gh` is not installed — print the PR URL for manual creation on GitHub.

## Step 7: Report

Show:
- PR URL
- Branch name
- Files changed
- Next steps (review, merge)

---

## Rules

- Never skip safety gate
- Never commit without user approval
- Never push to `main` directly
- If `gh` is not installed — fall back to printing the PR URL manually
