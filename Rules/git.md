# Rules: Git

## Remote

- Repo: https://github.com/Gredja/AiTest.git
- Branch: `main`

## Branches

- `main` — stable branch, do not touch directly
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
- `.github/workflows/nightly-tests.yml` — informational full suite: cron `17 3 * * *` UTC
  (off the hour — GitHub delays/drops top-of-hour schedules) + `workflow_dispatch`; sequential
  live suite inside the Playwright container; UI storageState refreshed in-run by
  `LoginSignInTests`, never stored; cron auto-disables after 60 days of repo inactivity (public repos)
- Nightly red is a drift/flake report for the owner — it never blocks merges; the live suite is
  NEVER a required check (rate-limit exhaustion and external-API instability are the documented
  Top-3 risks — `documentation/GitHubTestingStructure.md`)
- Repo secrets: `GITHUB_PAT`, `GITHUB_UI_EMAIL`, `GITHUB_UI_PASSWORD`, `GITHUB_UI_PAT` — values
  live only in GitHub settings, never in the repo
- Artifacts: test report + allure 7 days; failure UI traces 3 days only (traces can embed
  credentials — keep retention short)

## Secrets

- Token stored in `.env` (not tracked by git)
- Never commit `.env` or `.credentials`
- Never put tokens in commit messages

## Repository hygiene

- The repo root holds ONLY project artifacts (configs, `AGENTS.md`, `README.md`, solution).
  Tool outputs, caches, logs, and foreign projects MUST NOT live in the root.
- Generated tool output goes to a gitignored folder or `%TEMP%` — never hand-placed in the tree.
- When introducing a tool that writes files, add its ignore entries in the SAME commit.
- After scripted/bulk edits run `git add -A` and verify ZERO line-ending warnings —
  respect `.gitattributes` (.cs/.csproj → CRLF, .md → LF).
