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

## Loops

- **Sync side-effect loop over a `List<T>` → `list.ForEach(item => ...)`**, not `foreach` (per-item asserts, register); one statement → inline lambda, several assertions → block lambda; lambda parameter is a readable singular noun
- `foreach` stays only for: `async` bodies (teardown cleanup — `ForEach` has no async overload and `Task.WhenAll` would change ordering/parallelism), lazy/`IEnumerable`/array sources (no `ForEach` method — `File.ReadLines`, `Where(...)`, `Keys.OrderBy(...)`), iterators (`yield` is illegal inside a lambda), and multi-statement bodies with control flow (switch/if-heavy)

## Regex

- Prefer plain predicates/`char`-range checks when they read better — a pattern exists only when the predicate would be more obscure (precedent: `IsGitSha` replaced `^[0-9a-f]{40}$`)
- A pattern lives in a single named `static readonly` field, never inline in a call
- Every pattern carries a one-line comment above it saying in words what it matches (sample match for non-obvious ones)
- Verbatim `@"..."` for patterns with escapes/groups; `RegexOptions.Compiled` for patterns reused across tests
- The same pattern in 2+ files → extract to the shared base or a helper (precedent: `TopicNamePattern` in `GitHubTestBase`)

## Strings

- Use interpolation `$"Hello {name}"` over concatenation `"Hello " + name`
- Use `StringBuilder` when building strings in loops
- Use `string.IsNullOrEmpty()` or `string.IsNullOrWhiteSpace()` over `.Length == 0`

## Magic strings

- Extract a string to a `const` if it appears **2+ times** in the codebase
  - Occurrences must carry the **same meaning**: coincidentally identical texts (e.g. `Description` strings reused across services, the same path in different services' endpoint configs) do not count
- Group related constants in a dedicated static class (e.g. `AllureConstants`)
- Single-use domain constants go in the class that uses them
- Fallback/default values (`"Tests"`, `"Unknown"`) stay inline — they are self-explanatory
- Don't extract strings that are just "initialize + read in the same method" — that's one logical usage
- Extract regex patterns to `private static readonly Regex` fields — never inline `Regex.IsMatch()` in test assertions
- Extract test data with special chars (HTML, Unicode, etc.) to named constants — never inline in test code

## Null safety

- Use `?.` for safe navigation, `??` for fallback
- Avoid `null!` — if a value must exist, initialize or throw early
- Prefer pattern matching: `if (x is not null)` over `if (x != null)`

## Pattern matching

- Use `switch` expressions over `switch` statements when each case returns a value
- Use `is` type pattern instead of casting: `if (attr is RequiredFieldAttribute)` instead of `if (attr is RequiredFieldAttribute) { var rfa = (RequiredFieldAttribute)attr; ... }`
- Destructure in the pattern when you need the typed variable: `case ValueRangeAttribute range:`
