---
name: review-pr
description: Use when the user says "review-pr", "/review-pr", or wants to review a pull request. Automated code review for Gredja pull requests.
---

# Review PR — Gredja

All steps executed directly by the main agent. No subagent.

## Step 1: Gather PR info

Ask user for PR number if not provided.

Verify prerequisites:
- GitHub PAT with `repo` scope in `.env` as `GITHUB_PAT=github_pat_...`
- PowerShell (Invoke-RestMethod)

## Step 2: Fetch PR data

```powershell
$token = & Scripts/read-github-pat.ps1
$headers = @{ Authorization = "Bearer $token"
               Accept = 'application/vnd.github+json'
               'X-GitHub-Api-Version' = '2022-11-28' }
$pr = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/pulls/{pr_number}" -Headers $headers
$files = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/pulls/{pr_number}/files" -Headers $headers
```

For each changed file (modified/added), fetch content:
```powershell
$content = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/contents/<PATH>?ref=<HEAD_SHA>" -Headers $headers
$decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($content.content))
```

## Step 3: Read rules

Read `Rules/*.md` for full project rules context.

## Step 4: Review each changed file

Rules are read from `Rules/*.md` in Step 3 — this skill does NOT restate their text (a
restatement drifts the moment a rule file is edited; single source of truth, same principle
as /review and /commit). Match each changed file to the rules by subject and re-open the
Rules file whenever wording decides a finding:

| File kind | Rules to apply |
|---|---|
| any `.cs` | `code.md`, `code-style.md`, `code-principles.md`, `comments.md` |
| models / config | `models.md`, `config.md` |
| tests | `assertions.md`, `test-practices.md`, `categories.md` |
| `Ui/**`, moodledemo | additionally `ui-testing.md`, `localization-testing.md` |
| every file | `git.md` (secrets/branch hygiene), `workflow.md` |

Review-only checks (NOT rule text — keep as-is):
- CI `gate` check passed (format + HealthCheck) — read the PR check-runs; never infer build health from the diff alone
- No dead code, no duplicate code
- New code has test coverage

## Step 5: Draft the review comment (approval gate)

Compose the full review comment (markdown: verdict, issues, positives) and show it to the
user. **STOP — do not post anything yet.** Posting to a PR is visible to everyone
(blast radius: shared state), so it happens only on explicit user approval. If the user
declines or asks for edits → apply them, show again, wait; posting stays skipped until
approved.

## Step 6: Post the review comment (only after Step 5 approval)

```powershell
$body = @{ body = "<approved review markdown>" } | ConvertTo-Json
Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/issues/{pr_number}/comments" -Method Post -Headers $headers -Body $body
```

## Step 7: Report

- PR title and author
- Summary (1-2 sentences)
- Verdict: APPROVE / REQUEST_CHANGES / COMMENT
- Issues: numbered list with file:line and severity (blocking / suggestion / nit)
- Positives (brief)

---

## Rules

- Never skip code review
- Review against `Rules/*.md` files read in Step 3 — never against a restated checklist (single source of truth)
- Severity levels: blocking (must fix) / suggestion (should consider) / nit (optional)
- Posting a PR comment requires explicit user approval of the drafted comment (Step 5) — never post unprompted
- GitHub API headers: `Bearer` + `Accept: application/vnd.github+json` + `X-GitHub-Api-Version: 2022-11-28` (same style as /merge and /nightly-check)
- If GitHub PAT is missing — report and stop

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-11-08 | Step 5 → approval gate (draft → STOP → post); Step 2 headers unified (Bearer + 2022-11-28); Rules + Revision History added (S3) | MiMo |
| 1.1 | 2026-11-08 | Step 4: restated rule checklist replaced with subject→file mapping table (Rules stay the single source of truth; CI-gate/coverage/dead-code checks kept as review heuristics) (S6) | MiMo |
