# Rules: Code Style

## Error handling

- Catch specific exceptions (`InvalidOperationException`), not `Exception`
- Don't use exceptions for flow control — use `TryParse`, null checks, `Any()` instead
- Don't catch exceptions silently — either handle or let propagate
- Don't catch and rethrow without adding context — either handle or let propagate
- Use `nameof()` for argument exceptions instead of string literals

## LINQ

- Prefer `Any()` over `Count() > 0`
- Avoid `.ToList()` inside a chain — materialize only when you need to iterate multiple times
- Don't do multiple iterations over the same collection — chain or materialize once
- Use `FirstOrDefault()` over `Where(...).FirstOrDefault()` when looking for one item

## Strings

- Use interpolation `$"Hello {name}"` over concatenation `"Hello " + name`
- Use `StringBuilder` when building strings in loops
- Use `string.IsNullOrEmpty()` or `string.IsNullOrWhiteSpace()` over `.Length == 0`

## Null safety

- Use `?.` for safe navigation, `??` for fallback
- Avoid `null!` — if a value must exist, initialize or throw early
- Prefer pattern matching: `if (x is not null)` over `if (x != null)`
