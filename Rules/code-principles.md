# Rules: Code Principles

## General

- No access modifier = `private` (e.g. `static RestClient` → `private static RestClient`)
- Empty line before `return`
- Simplify when possible: `var x = new T(); return x;` → `return new T();`
- No magic numbers or strings — extract to constants
- No nested ternaries — use `if`/`switch`
- Always use `{}` for `if` blocks, even single-line

## SOLID principles

- **S**ingle Responsibility — one class, one job. If you're using "and" to describe what a class does, split it.
- **O**pen/Closed — extend via interfaces/base classes, not by modifying existing code.
- **L**iskov Substitution — subclasses must be usable wherever the parent is expected.
- **I**nterface Segregation — many small interfaces over one fat interface.
- **D**ependency Inversion — depend on abstractions (interfaces), not concrete classes. Inject via constructor.
