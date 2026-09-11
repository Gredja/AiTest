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

## Magic strings

- Extract a string to a `const` if it appears **3+ times** in the codebase
- Group related constants in a dedicated static class (e.g. `AllureConstants`)
- Single-use domain constants go in the class that uses them
- Fallback/default values (`"Tests"`, `"Unknown"`) stay inline — they are self-explanatory
- Don't extract strings that are just "initialize + read in the same method" — that's one logical usage

## Null safety

- Use `?.` for safe navigation, `??` for fallback
- Avoid `null!` — if a value must exist, initialize or throw early
- Prefer pattern matching: `if (x is not null)` over `if (x != null)`

## Pattern matching

- Use `switch` expressions over `switch` statements when each case returns a value
- Use `is` type pattern instead of casting: `if (attr is RequiredFieldAttribute)` instead of `if (attr is RequiredFieldAttribute) { var rfa = (RequiredFieldAttribute)attr; ... }`
- Destructure in the pattern when you need the typed variable: `case ValueRangeAttribute range:`
