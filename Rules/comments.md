# Rules: Comments

## Default: no comments

Code must speak for itself through clear names and structure.

## When comments ARE needed

- **Regex** — explain what the pattern matches
- **TODO** — during development only, must be removed before merge to main
- **Non-obvious WHY** — workaround for a specific bug, hidden business constraint

## What NOT to comment

- What the code does (names already say that)
- Obvious logic
- TODO/FIXME without context (allowed during development, must have context)
- AAA blocks (Arrange/Act/Assert) — structure speaks for itself
- Section dividers (`// ====`) — noise
