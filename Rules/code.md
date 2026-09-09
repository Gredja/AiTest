# Rules: Code Writing

## Naming

- PascalCase for classes, methods, properties, constants
- camelCase for local variables and parameters
- `_camelCase` for private fields
- No abbreviations in names (`response` not `resp`, `productId` not `pid`)
- Boolean variables/methods: prefix with `Is`, `Has`, `Can`, `Should`

## Types

- Use file-scoped namespaces (`namespace X;`)
- One class per file, file name matches class name
- Use `var` wherever possible — only use explicit type when `var` is not applicable (e.g. `null`, tuples, primitives without assignment)

## File Layout

- `using` directives at the top, outside namespace
- One blank line between `using` block and namespace
- One blank line between members
- No trailing whitespace

## Methods

- Keep methods short and focused — one responsibility
- Max ~30 lines; extract helper if longer
- Max 5 parameters; use object/record for more

## Async

- All API requests must be asynchronous: use `async Task<...>` methods with `await`
- Synchronous RestSharp methods (`Execute`, `Execute<T>`) are not allowed — use `ExecuteAsync`, `ExecuteAsync<T>`
- Test methods returning `Task` must be `async Task`, not `void`

## General

- No access modifier = `private` (e.g. `static RestClient` → `private static RestClient`)
- Empty line before `return`
- Simplify when possible: `var x = new T(); return x;` → `return new T();`
- No magic numbers or strings — extract to constants
- No nested ternaries — use `if`/`switch`
- Always use `{}` for `if` blocks, even single-line
- Don't catch exceptions silently — either handle or let propagate
- Use `nameof()` for argument exceptions instead of string literals

## Access modifiers (review rule)

During any code review: check that every method, property, and field has the **narrowest possible** access modifier.

- If used only inside the class → `private`
- If used only by subclasses → `protected`
- If used by external callers → `public`

Default to `private`. Only widen when there's a concrete reason.
