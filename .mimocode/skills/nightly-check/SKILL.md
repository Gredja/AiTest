---
name: nightly-check
description: Check the latest nightly test run (nightly-tests.yml) on GitHub and give a one-line verdict — run number, conclusion, duration, link; on failure also pulls job step results. Use when the user says "/nightly-check", "проверь ночной прогон", "ночной прогон как", "зелёный ли nightly", "nightly status", or at session start when the project memory rule asks to check the nightly run first.
---

# Nightly Check

Check the last run of the nightly suite in `Gredja/AiTest` and report the verdict.

## Instructions

### Step 1: Token (never printed)

Read `GITHUB_PAT` from the process environment via `Scripts/read-github-pat.ps1` — the
redaction contract (the token must never appear in output, logs, or messages) is
documented there, in exactly one place.

If the read throws (variable not set — secrets are not persisted locally by design, so a
fresh shell needs its own load): run `Scripts/sync-secrets.ps1` (Infisical pull into the
current process memory, output = key names only, never values), then read the token:

```powershell
./Scripts/sync-secrets.ps1
$token = & Scripts/read-github-pat.ps1
```

### Step 2: Fetch the latest run

```powershell
$token = & Scripts/read-github-pat.ps1
$headers = @{ Authorization = "Bearer $token"
               Accept = 'application/vnd.github+json'
               'X-GitHub-Api-Version' = '2022-11-28' }
$runs = Invoke-RestMethod `
  -Uri 'https://api.github.com/repos/Gredja/AiTest/actions/workflows/nightly-tests.yml/runs?per_page=1' `
  -Headers $headers
$run = $runs.workflow_runs[0]
```

Expected fields: `run_number`, `status`, `conclusion`, `event`, `created_at`, `html_url`.

### Step 3: Verdict

- `status -ne 'completed'` → poll the same run (`/actions/runs/{id}`) every 30 s,
  max 5 minutes, then report the current state as `in progress`.
- `conclusion -eq 'success'` → one green line:
  `Nightly #<run_number> ✅ success (<event>, <created_at>) — <html_url>`
- `conclusion -eq 'failure'/'timed_out'` → Step 4 (failure dig) and report red.

### Step 4: Failure dig

```powershell
$jobs = Invoke-RestMethod `
  -Uri "https://api.github.com/repos/Gredja/AiTest/actions/runs/$($run.id)/jobs" `
  -Headers $headers
```

Report per job: name, conclusion; per step: name + conclusion (highlight the
`Run suite two-pass (API + E2E + MoodleDemo UI, excl. GitHubUi)` step — for it, note that a sequential rerun inside the
script means persistent failures are real and flaky races self-heal).

Then offer (do not run unless the user says so): download the `test-run-report` artifact
and open the `## Failed tests` section of the newest `TestResults/TestRunReport-*.md`.

## Important

- This check is read-only — never re-dispatch, cancel, or delete runs.
- One-line verdict first, details only on failure or when asked.
- On session start (project MEMORY rule) run this proactively right after the greeting.

## Troubleshooting

- `401` on the API → token missing/renamed: run `Scripts/sync-secrets.ps1` (fresh Infisical pull into process memory), then read the token again in the same invocation.
- `404` on the workflow → workflow file not on `main` yet (`.github/workflows/nightly-tests.yml`).
- Empty `workflow_runs` → the workflow never ran; suggest `workflow_dispatch` from the Actions tab.
- Rate limit on api.github.com itself → report `X-RateLimit-Remaining` from the error headers, retry once after a minute.

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.2 | 2026-11-16 | Secrets architecture: no local persistence (setx/user-scope cache removed) — `sync-secrets.ps1` loads into process memory only; Step 1 fallback + 401 troubleshooting reworded | MiMo |
| 1.1 | 2026-11-16 | Step 1 fallback: on a missing `GITHUB_PAT` run `Scripts/sync-secrets.ps1` + user-scope re-read into the current process; 401 troubleshooting aligned (session restart not required for this check) | MiMo |
| 1.0 | 2026-11-08 | Revision History added (S9); no functional changes — skill reviewed, no defects found | MiMo |
