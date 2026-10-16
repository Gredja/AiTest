# Rules: Git

## Remote

- Repo: https://github.com/Gredja/AiTest.git
- Branch: `main`

## Branches

- `main` — stable branch, do not touch directly
- `main` is protected (2026-10-16): PR required (0 approvals) + required check `gate` with
  strict up-to-date, no force pushes/deletions; `enforce_admins` is off — the admin can bypass
  in a real emergency
- All changes go into `features/...` branches
- Format: `features/<topic>`

## Commits

- Only on user request
- English language
- Format: **action + object** (e.g. "Add git rules", "Fix product tests")
- Ask the user for the commit message (propose one based on style)

## Pull Requests

PRs from `features/...` into `main` — only after review and approval.

## CI (GitHub Actions)

- `.github/workflows/pr-checks.yml` — required PR gate on `main`: `format` runs on every PR;
  live `HealthCheck` only for same-repo branches (fork PRs get an explicit stub — secrets are
  unavailable to forks, and a missing required check would pend forever and block merges)
- `.github/workflows/nightly-tests.yml` — informational API + E2E suite (`--filter
  "Category!=GitHubUi"`): cron `17 3 * * *` UTC (off the hour — GitHub delays/drops top-of-hour
  schedules) + `workflow_dispatch`; two-pass via `Scripts/test-parallel-rerun.ps1` (`shell: pwsh`
  on `ubuntu-latest` + setup-dotnet; no container — browsers are not needed once the UI leg is
  excluded): pass 1 in 8 fixture workers, failed tests rerun sequentially (1 worker); only
  persistent failures fail the run (flaky races self-heal, drift stays red); clean clone gets CI
  `testsettings.json` with `Headless: true` generated from the example before build; cron
  auto-disables after 60 days of repo inactivity (public repos)
- **UI leg excluded (2026-10-15, user decision):** GitHub device verification (email OTP) fires
  on the `gredjapl` web login from CI runner IPs, so the in-run `LoginSignInTests` storageState
  refresh can never complete — nightly runs `Category!=GitHubUi`, UI suite stays local-only
  (alternatives storageState-secret / OTP automation rejected)
- Nightly red is a drift/flake report for the owner — it never blocks merges; the live suite is
  NEVER a required check (rate-limit exhaustion and external-API instability are the documented
  Top-3 risks — `documentation/GitHubTestingStructure.md`)
- **Workflow YAML gotcha:** GitHub's parser rejects a `: ` (colon-space) inside an unquoted
  `run:` scalar — the workflow silently falls back to the filename as its name and
  `workflow_dispatch` returns 422 "does not have 'workflow_dispatch' trigger". Any command
  containing `: ` (curl headers, sed patterns) must use the block scalar `run: |`
- Repo secrets — Actions rejects the `GITHUB_` prefix in secret names, so the names are
  `TEST_PAT`, `UI_EMAIL`, `UI_PASSWORD`, `UI_PAT`; each workflow maps them to the env vars the
  code reads (`GITHUB_PAT`, `GITHUB_UI_EMAIL`, `GITHUB_UI_PASSWORD`, `GITHUB_UI_PAT`) in its
  `env:` block — values live only in GitHub settings, never in the repo
- Artifacts: test report + allure 7 days (UI traces are not uploaded — UI suite is local-only)

## Secrets

- Token stored in `.env` (not tracked by git)
- Never commit `.env` or `.credentials`
- Never put tokens in commit messages

## Repository hygiene

- The repo root holds ONLY project artifacts (configs, `AGENTS.md`, `README.md`, solution).
  Tool outputs, caches, logs, and foreign projects MUST NOT live in the root.
- `documentation/Katas/` is training material — never modify, move, refactor or include it
  in audits; read-only reference (2026-10-16)
- Generated tool output goes to a gitignored folder or `%TEMP%` — never hand-placed in the tree.
- When introducing a tool that writes files, add its ignore entries in the SAME commit.
- After scripted/bulk edits run `git add -A` and verify ZERO line-ending warnings —
  respect `.gitattributes` (.cs/.csproj → CRLF, .md → LF).
