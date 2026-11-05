---
name: merge
description: Finish the current branch end-to-end — commit pending changes via /commit, open a PR to main (title composed from the changes), wait for all GitHub gates green, merge the PR, delete the branch on the remote AND locally, fast-forward local main. Use when the user says "/merge", "смержи", "влий ветку в main", "закрой ветку", "сделай merge", "прогони до слияния".
---

# Merge

Drive the current feature branch all the way into `main`. Hard guards: never target a base
other than `main`, never push to `main` directly, never merge with a red gate.

## Instructions

Run everything yourself; polling waits happen in a background subagent — no blocking loops
in the main thread. The token from `.env` (`GITHUB_PAT`) is never printed.

### Step 1: Preconditions

```powershell
git branch --show-current        # must NOT be main → if main: report and STOP
git status --short
```

- On `main` → STOP (nothing to merge into itself).
- If the working tree is dirty (or untracked files exist) → Step 2 (`/commit` skill, full flow:
  gates, rules review, backups sync, `normalize-eol.ps1`).
- If clean → skip to Step 3 with what is already committed.

### Step 2: Commit (via /commit only)

Invoke the `/commit` skill and follow it exactly. If it stops (gates red, no approval) →
report and STOP this skill too.

### Step 3: Push and open the PR

```powershell
git push -u origin HEAD
```

Gather the base for the PR body: `git log --oneline origin/main..HEAD` (title = first line
style, action + object; compose a full title yourself, ≤70 chars). Body = bullet summary of
the commits + verification lines (format / HealthCheck / suites touched).

Create via the API (no `gh` locally):

```powershell
$token = & Scripts/read-github-pat.ps1
$headers = @{ Authorization = "Bearer $token"
               Accept = 'application/vnd.github+json'
               'X-GitHub-Api-Version' = '2022-11-28' }
$payload = @{ title = '<composed title>'; head = '<branch>'; base = 'main'; body = $body } | ConvertTo-Json
# POST https://api.github.com/repos/Gredja/AiTest/pulls  (curl --data-binary @file is the
# reliable encoder if Invoke-RestMethod reports a JSON parse error)
```

If a PR for this branch already exists → reuse it (skip creation).

### Step 4: Wait for ALL gates

Spawn a background watcher subagent: poll
`GET /repos/Gredja/AiTest/pulls/{n}` (→ `head.sha`) then
`GET .../commits/{sha}/check-runs` every 15 s (limit 8 min) until every check completes.

- Any `conclusion != success` → collect names/conclusions → report RED and STOP (never merge,
  never re-run blindly; offer analysis as a next step).
- All `success` → continue.

### Step 5: Merge

```powershell
# PUT /repos/Gredja/AiTest/pulls/{n}/merge   body: {"merge_method":"merge"}
```

Verify `merged: true`; record the merge sha. If the API rejects (405/behind) → report the
reason (conflicts / required checks) and STOP — no force options.

### Step 6: Delete the merged branch (remote + local) and update main

Remote:

```powershell
# DELETE /repos/Gredja/AiTest/git/refs/heads/<branch>   (204; 404 = already gone, OK)
```

Verify with `git ls-remote --heads origin <branch>` (empty = gone).

Local — always, no reminder needed (user rule, 2026-11-04):

```powershell
git switch main
git pull --ff-only
git branch -d <branch>    # -d only (never -D): refuses to delete an unmerged branch
```

Never delete `main` itself.

### Step 7: Report

**Slack notification first:** call the `slack-local` MCP tool `post_message` with text
`✅ merge PR #<n>: <title> → <merge sha>` — omit `channel` (the server takes
`SLACK_CHANNEL` from `.env`). A failed or unavailable notification is reported below and
never blocks the merge flow.

In execution order: commit (message/hash) or "skipped — tree clean" · PR number/url/title ·
gates result (names + verdict) · merge sha · branch deleted (remote + local) · local main
fast-forwarded · Slack notification (sent / failed).

## Important

- Failure at any step → report exactly where and why, keep the branch intact, and stop.
- All commits inside this flow go through the `/commit` skill — this skill never invents its
  own `git commit`.
- Gates include the PR `gate` check (format + HealthCheck) — local green does not replace it.
- Branch cleanup is part of this flow (Step 6): the agent deletes the merged branch remotely
  AND locally and fast-forwards local `main` itself — no user action, no warning-only reports.

## Troubleshooting

- `422` on PR creation → malformed JSON body: write the payload to a temp file and send with
  `curl --data-binary "@file"`.
- Gate never completes → check for a queued/stuck run under Actions; report, don't wait forever
  (watcher limit 8 min).
- `405` on merge → branch has conflicts or required checks not met; surface the API `message`.
- Branch already merged before Step 6 → treat remote `404` as success.
