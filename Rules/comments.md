# Rules: Comments

## Default: no comments

Code must speak for itself through clear names and structure.

## Comments are a necessary evil

A comment is the LAST resort. First refactor for clarity (extract, rename, restructure);
add a comment only when the code cannot be refactored further and is still unclear.
A comment that explains WHAT the code does means the code needs refactoring, not a comment.

## When comments ARE needed

- **Regex** — extract pattern to `private static readonly Regex` field with `RegexOptions.Compiled`; never inline in test assertions
- **TODO** — during development only, must be removed before merge to main
- **Non-obvious WHY** — workaround for a specific bug, hidden business constraint

## What NOT to comment

- What the code does (names already say that)
- Obvious logic
- TODO/FIXME without context (allowed during development, must have context)
- AAA blocks (Arrange/Act/Assert) — structure speaks for itself
- Section dividers (`// ====`) — noise
