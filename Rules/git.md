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

## Secrets

- Token stored in `.env` (not tracked by git)
- Never commit `.env` or `.credentials`
- Never put tokens in commit messages
