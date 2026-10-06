# Rules: Code Writing

## Naming

- PascalCase for classes, methods, properties, constants
- camelCase for local variables and parameters
- `_camelCase` for private fields
- No abbreviations in names (`response` not `resp`, `productId` not `pid`)
- Boolean variables/methods: prefix with `Is`, `Has`, `Can`, `Should`
- Lambda parameters: readable singular noun matching the element type (`product => product.Id`, `comment => comment.PostId`), never single-letter (`p =>`, `c =>`)

## Types

- Use file-scoped namespaces (`namespace X;`)
- One class per file, file name matches class name
- Use `var` wherever possible — only use explicit type when `var` is not applicable (e.g. `null`, tuples, primitives without assignment)

## File Layout

- `using` directives at the top, outside namespace
- One blank line between `using` block and namespace
- One blank line between members
- No trailing whitespace
- **Helper placement** — `Api` and `E2E` both reference `Core`, but neither references the other:
  - Request-infrastructure (`RequestHelper` + client layers `FakeStoreRequestHelper`, `JsonPlaceholderRequestHelper`, `GitHubRequestHelper`) → always `Core/Helpers/Http/`, regardless of E2E usage
  - Used by E2E too (e.g. `GitHubTestBase`, `GitHubParamHelper`) → `Core/Helpers/` (or `Core/Helpers/<Service>/`)
  - Used only by API tests (e.g. `FakeStoreParamHelper`, `JsonPlaceholderParamHelper`) → `Api/<Service>/Helpers/`
  - Shared param-building primitives (`ParamHelper`: `IdParam`, `UrlSegment`, `Query`) → `Core/Helpers/Params/ParamHelper.cs`
  - Never place E2E-needed code in `Api/` — the E2E project has no reference to `Api`

## Cleanup

- **Remove unused usings** — when generating or modifying code, always clean up unused `using` directives in the same turn. Never leave dead imports behind.
- **NUnit globally imported** — `Api.csproj` and `E2E.csproj` have `<Using Include="NUnit.Framework" />`. Never add explicit `using NUnit.Framework;` in these projects.
- **Check inheritance** — before removing a `using`, verify the type isn't needed for base class resolution (e.g. `using Core.Helpers.GitHub;` may be needed for `: GitHubTestBase`).

## Methods

- Keep methods short and focused — one responsibility
- Max ~30 lines; extract helper if longer
- Max 5 parameters; use object/record for more

## Async

- All API requests must be asynchronous: use `async Task<...>` methods with `await`
- Synchronous RestSharp methods (`Execute`, `Execute<T>`) are not allowed — use `ExecuteAsync`, `ExecuteAsync<T>`
- Test methods returning `Task` must be `async Task`, not `void`

## Access modifiers (review rule)

During any code review: check that every method, property, and field has the **narrowest possible** access modifier.

- If used only inside the class → `private`
- If used only by subclasses → `protected`
- If used by external callers → `public`

Default to `private`. Only widen when there's a concrete reason.
