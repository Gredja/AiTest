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

## Repository hygiene

- The repo root holds ONLY project artifacts (configs, `AGENTS.md`, `README.md`, solution).
  Tool outputs, caches, logs, and foreign projects MUST NOT live in the root.
- Generated tool output goes to a gitignored folder or `%TEMP%` — never hand-placed in the tree.
- When introducing a tool that writes files, add its ignore entries in the SAME commit.
- After scripted/bulk edits run `git add -A` and verify ZERO line-ending warnings —
  respect `.gitattributes` (.cs/.csproj → CRLF, .md → LF).
