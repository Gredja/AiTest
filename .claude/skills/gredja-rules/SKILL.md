# Skill: Gredja Rules

All project rules for Gredja — .NET 10.0 test automation solution (NUnit, RestSharp, Playwright). FakeStoreAPI for API testing.

---

## Workflow

All changes require a plan first, then user approval, then execution.

1. Show the plan (what files, what changes)
2. Wait for user approval
3. Make changes
4. Show a brief report of what was done
5. After commit — review `Rules/` and suggest updates if needed
6. After structural changes — update `FILE_STRUCTURE.md`

---

## Git

- Repo: https://github.com/Gredja/AiTest.git
- Branch: `main`
- All changes go into `features/<topic>` branches
- **Commits:** only on user request, English, format: action + object (e.g. "Add git rules")
- PRs from `features/...` into `main` — only after review
- Never commit `.env` or tokens

---

## Models

### Response models — suffix `Model`

- Always includes `Id` field (server-generated)
- May include nested models (e.g. `RatingModel` inside `ProductModel`)
- Match the full JSON structure from the API response

### Request models — suffix `Request`

- No `Id` field (server generates it)
- Only fields the client must provide
- For PUT — may include `Id` if the endpoint expects it in the body

### Property rules

| Type | Rule | Example |
|------|------|---------|
| Reference (string, object, List) | No `?`, no initialization | `public string Title { get; set; }` |
| Value (int, decimal, double, DateTime) | Add `?` only if JSON field can be null/absent | `public DateTime? Date { get; set; }` |

### Namespaces

- Regular models: `Core.Models`
- Reusable generics: `Core.Models.Generic`

### What NOT to do

- Don't add constructors unless needed for deserialization
- Don't add validation attributes (validation is in tests, not in models)
- Don't add methods or logic — models are pure data containers
- Don't use `init` — use `{ get; set; }` for all properties

---

## Assertions

**Library:** FluentAssertions (not NUnit Assert)

### Key patterns

- `x.Should().Be(y)` — equals
- `x.Should().NotBeNull()` — not null
- `x.Should().NotBeNullOrWhiteSpace()` — string not null/empty
- `x.Should().BeGreaterThan(y)` — comparison
- `x.Should().BeInRange(a, b)` — range check
- `collection.Should().OnlyContain(p => ...)` — all items match predicate
- `x.Should().NotBeEmpty()` — collection not empty

### Notes

- `Assert.Multiple` not needed — FluentAssertions gives clear error messages
- Avoid `out _` inside `OnlyContain` lambdas (expression tree limitation)

---

## Comments

**Default: no comments.** Code must speak for itself through clear names and structure.

### When comments ARE needed

- **Regex** — explain what the pattern matches
- **Non-obvious WHY** — workaround for a specific bug, hidden business constraint

### What NOT to comment

- What the code does (names already say that)
- Obvious logic
- TODO/FIXME without context
- AAA blocks (Arrange/Act/Assert) — structure speaks for itself
- Section dividers (`// ====`) — noise

---

## Code Writing

### Naming

- PascalCase for classes, methods, properties, constants
- camelCase for local variables and parameters
- `_camelCase` for private fields
- No abbreviations (`response` not `resp`, `productId` not `pid`)
- Boolean variables/methods: prefix with `Is`, `Has`, `Can`, `Should`

### Types

- File-scoped namespaces (`namespace X;`)
- One class per file, file name matches class name
- Prefer explicit types over `var` when type is not obvious
- `var` is fine when type is clear: `var request = new RestRequest(...)`

### Methods

- Keep methods short — one responsibility
- Max ~30 lines; extract helper if longer
- Max 3-4 parameters; use object/record for more

### General

- No magic numbers or strings — extract to constants
- No nested ternaries — use `if`/`switch`
- Don't catch exceptions silently — either handle or let propagate
- Use `nameof()` for argument exceptions

---

## Config

- Base URL and all endpoint paths in `Core/Config/Endpoints.cs`
- Never hardcode URLs or paths in tests — use constants from `Endpoints`

---

## Non-Existent ID Tests

- 0 and -1 are safe static IDs (always invalid)
- For "not found" tests — dynamic: GET all → find maxId → use maxId + 1 (never static 999)

---

## FakeStoreAPI Behavior

- Returns 200 OK (not 404) for non-existent product IDs (0, -1, maxId+1)
- Mark such tests with `[Ignore]`

---

## Test Base Class

- `ApiTestBase` in `Api/ApiTestBase.cs` — all API test fixtures inherit from it
- TearDown auto-collects metrics via `ApiEndpointAttribute` reflection
