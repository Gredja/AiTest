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
- Provenance notes next to locators (`verified 2026-…`) — the Observable Behaviour owns that

## Decision procedure (before keeping a comment)

1. Delete it — did the code become unclear or dangerous? No → delete.
2. Fixable by code (reference a constant, better name)? Fix the code, delete the comment.
3. Still an undeniable WHY (concrete bug workaround, hidden constraint, danger) that code
   cannot express? Keep it, as short as possible.

NEVER write excess comments — zero is the default for every line of new code.
