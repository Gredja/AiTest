---
name: pr
description: Use when the user says "pr", "/pr", or wants to create a pull request. Creates a branch, commits with safety checks, pushes, and opens a PR.
---

# PR Agent for Gredja

Orchestrate a PR via a subagent. The main agent gathers inputs; the subagent executes.

## Step 1: Status check (main agent)

Run `git status` and `git diff --stat`. Show:
- Current branch
- Changed files (staged + unstaged)
- Untracked files

If working tree is clean — report and stop.

Ask for confirmation to proceed.

## Step 2: Gather inputs (main agent)

### Branch name

If the user provided a branch name — use it (prefix with `features/` if missing).

Otherwise, suggest a branch name based on the changes (e.g. `features/add-product-tests`).

If already on a feature branch — confirm with user.

### Commit message

If the user provided a commit message — use it.

Otherwise, analyze the staged changes and propose a message: **action + object** (e.g. "Add product API tests", "Fix model nullable properties").

Show proposed message and ask for approval.

## Step 3: Spawn subagent (main agent)

Once user confirmed AND inputs gathered, spawn a `general` subagent with this prompt:

```
You are a PR subagent for Gredja. Execute the following steps precisely. Do NOT ask the user anything — all inputs are provided below.

Branch: {branch}
Commit message: {message}
Working dir: {working_dir}

## Steps

1. Branch:
   - If on main: run `git checkout -b {branch}`
   - If already on feature branch: confirm with `git branch --show-current`

2. Safety gate (HARD RULE — no exceptions):
   - Run `dotnet format --verify-no-changes`. If fails: run `dotnet format`, then re-verify.
   - Run `dotnet test --verbosity quiet`. If fails: report the failure and STOP. Do not commit.

3. Commit:
   - Run `git add -A`
   - Run `git diff --cached --stat` to confirm staged files
   - Run `git commit -m "{message}"`

4. Push:
   - Run `git push -u origin {branch}`

5. Create PR:
   - Run `gh pr create --title "{message}" --body "## Summary\n- {what changed}\n- {why}\n\n## Verification\n- [x] dotnet format passed\n- [x] dotnet test passed"`
   - If `gh` is not installed — report the PR URL for manual creation on GitHub.

6. Report:
   - PR URL (or manual URL)
   - Branch name
   - Commit hash (from git log -1 --format="%H")
   - Files changed
   - Commit message
   - Next steps (review, merge)
```

## Step 4: Deliver result (main agent)

Report the subagent's output to the user.

---

## Rules

- Never skip safety gate
- Never commit without user approval (gathered in Step 1-2 before spawning)
- Never push to `main` without explicit confirmation
- Commits: English only, format: action + object
- Never commit `.env` or tokens
- If `gh` is not installed — fall back to printing the PR URL manually
