# Rules: Code Principles

## General

- No access modifier = `private` (e.g. `static RestClient` → `private static RestClient`)
- Empty line before `return`
- Simplify when possible: `var x = new T(); return x;` → `return new T();`
- No magic numbers or strings — extract to constants
- No nested ternaries — use `if`/`switch`
- Always use `{}` for `if` blocks, even single-line

## Guard clauses

- Use early return to flatten nested `if` blocks
- Validate preconditions first, then proceed with the main logic
- Invert condition: `if (condition) return;` instead of `if (!condition) { /* main logic */ }`

## Expression-bodied members

- Use `=>` for single-expression methods, properties, and lambdas
- Example: `protected void UseGitHub() => Client = _gitHubClient;`

## SOLID principles

- **S**ingle Responsibility — one class, one job. If you're using "and" to describe what a class does, split it.
- **O**pen/Closed — extend via interfaces/base classes, not by modifying existing code.
- **L**iskov Substitution — subclasses must be usable wherever the parent is expected.
- **I**nterface Segregation — many small interfaces over one fat interface.
- **D**ependency Inversion — depend on abstractions (interfaces), not concrete classes. Inject via constructor.
  **Accepted debt (audit F09, user decision 2026-10-19):** this test project intentionally has
  ZERO hand-rolled interfaces/DI — `RequestHelper`, endpoints and config are concrete statics.
  Do NOT file "no interfaces" as a violation and do NOT introduce DI for its own sake: seams
  pay for themselves only under unit tests of the core itself, which this suite does not have
  (everything runs against live/mock HTTP). Introduce an interface only when a concrete
  testable seam appears (e.g. clock, transport fakes) — never as a checklist refactor.
