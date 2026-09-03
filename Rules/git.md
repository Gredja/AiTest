# Rules: Git

## Remote

- Repo: https://github.com/Gredja/AiTest.git
- Branch: `master`

## Branches

- `master` — стабильная ветка, не трогаем напрямую
- Все изменения делаем в `features/...` ветках
- Формат: `features/<topic>`

## Commits

- English language
- Format: **action + object** (e.g. "Add git rules", "Fix product tests")
- Спрашивать коммит-месседж у пользователя

## Pull Requests

PR из `features/...` в `master` — только после ревью и апрува.

## Secrets

- Token stored in `.env` (not tracked by git)
- Never commit `.env` or `.credentials`
- Never put tokens in commit messages
