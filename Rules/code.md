# Rules: Code Writing

## Naming

- PascalCase for classes, methods, properties, constants
- camelCase for local variables and parameters
- `_camelCase` for private fields
- No abbreviations in names (`response` not `resp`, `productId` not `pid`)
- Boolean variables/methods: prefix with `Is`, `Has`, `Can`, `Should`
  - Exceptions: standard Try-pattern methods (`TryOwnRun`, `TryParse`, …); config getters named for the type they return (`GetRequiredBool`) when they are not boolean state
- Lambda parameters: readable singular noun matching the element type (`product => product.Id`, `comment => comment.PostId`), never single-letter (`p =>`, `c =>`)
- Helper classes: suffix `Helper` (`RetryHelper`, `WaitHelper`, `ArtifactNamingHelper`) — mandatory for
  every behavioral (method-bearing) class placed in a `Helpers/` layer. Pure constant holders and
  result records are not helpers and keep descriptive nouns (`UiTimeout`, `TestDataConstants`, `WaitResult`)
- Generator classes: suffix `Generator` (`DataGenerator`) — see File Layout: generators live in their
  own folder, never inside `Helpers/`

## Types

- Use file-scoped namespaces (`namespace X;`)
- One class per file, file name matches class name
- Use `var` wherever possible — only use explicit type when `var` is not applicable (e.g. `null`, tuples, primitives without assignment)

## File Layout

- **Before creating ANY file — decide its place first**: where it belongs by common sense AND
  future plans for the codebase — never drop it into the "closest existing folder" by inertia;
  name-vs-content must match, and the placement must still make sense as the project grows
  (source, docs, scripts, configs alike)
- `using` directives at the top, outside namespace
- One blank line between `using` block and namespace
- One blank line between members
- No trailing whitespace
- **Helper placement** — `Api` and `E2E` both reference `Core`, but neither references the other:
  - Request-infrastructure (`RequestHelper` + client layers `JsonPlaceholderRequestHelper`, `GitHubRequestHelper`) → always `Core/Helpers/Http/`, regardless of E2E usage
  - Used by E2E too (e.g. `GitHubTestBase`, `GitHubParamHelper`) → `Core/Helpers/` (or `Core/Helpers/<Service>/`)
  - Used only by API tests (e.g. `JsonPlaceholderParamHelper`) → `Api/<Service>/Helpers/`
  - Shared param-building primitives (`ParamHelper`: `IdParam`, `UrlSegment`, `Query`) → `Core/Helpers/Params/ParamHelper.cs`
  - Never place E2E-needed code in `Api/` — the E2E project has no reference to `Api`
- **Folder naming convention** — code-facing folders are PascalCase (`Core/`, `AllureAdapter/`,
  `Scripts/`); content/material folders at the repo root are lowercase (`documentation/`,
  `education/`, `research/`, `backups/`, `output/`); inside one layer keep a stable grammatical
  form — plural for collections (`Attributes/`, `Helpers/`, `Models/`), activity nouns
  (`Config/`, `Logging/`, `Reporting/`) are accepted as-is: never rename folders purely for
  grammatical symmetry; `Ui/github/` (lowercase service name) is a documented site-scoped
  exception — every other service folder is PascalCase (`Api/GitHub/`, `E2E/GitHub/`,
  `Core/Models/GitHub/`)
- **Generators live in their own folder** — classes with the `Generator` suffix go into a
  dedicated `Generators/` folder of their layer (e.g. `Core/Generators/`), never into
  `Helpers/` — helpers and generators are different roles with different name suffixes.
  Exception: a generator that is part of a domain module stays with that module
  (`Core/Reporting/TestRunReportGenerator` — its row DTO and retention companion live there too)

## Cleanup

- **Remove unused usings** — when generating or modifying code, always clean up unused `using` directives in the same turn. Never leave dead imports behind.
- **NUnit globally imported** — `Api.csproj` and `E2E.csproj` have `<Using Include="NUnit.Framework" />`. Never add explicit `using NUnit.Framework;` in these projects.
- **Check inheritance** — before removing a `using`, verify the type isn't needed for base class resolution (e.g. `using Core.Helpers.GitHub;` may be needed for `: GitHubTestBase`).

## Methods

- Keep methods short and focused — one responsibility
- Max ~30 lines; extract helper if longer
- **Test methods: readability wins over the limit** — an E2E/scenario test may exceed ~30 lines when splitting it into step-helpers would obscure the chain (create → verify → mutate → cleanup reads as one story). The limit still applies to helpers/infrastructure the tests call
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
