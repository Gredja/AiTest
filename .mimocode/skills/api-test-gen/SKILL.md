---
name: api-test-gen
description: Use when the user wants to generate API tests for any service endpoint. Trigger on mentions of "api test gen", "generate API tests", "/api-test-gen", or testing a new endpoint.
---

# API Test Generation

## Purpose

Generates a complete set of NUnit API tests for any REST API service endpoint — for AQA Engineer at the test automation stage.

---

## Variable Placeholders

| Placeholder | Description | Example value |
|---|---|---|
| `{{service_name}}` | API service (matches directory/class names) | FakeStore, JsonPlaceholder, Swapi |
| `{{endpoint_name}}` | Endpoint to test | users, posts, todos |

---

## Output Format Instruction

Model must return: test files (.cs), models (.cs), and constants (.cs) in C# format with namespace, class, methods. Format — code without comments. After generation — report: list of files, test count (total / positive / negative / ignored).

---

## Input

User provides:
1. **Service name** — `{{service_name}}` (must match existing directory name in `Api/`)
2. **Endpoint name** — `{{endpoint_name}}` (e.g. `products`, `users`, `posts`)

If not provided, ask which service and endpoint to test.

## File Convention (service-agnostic)

All paths use `{Service}` as the service name (PascalCase, matches directory names):

| What | Path pattern |
|---|---|
| Endpoints config | `Core/Config/{Service}Endpoints.cs` |
| Models | `Core/Models/{Service}/` |
| Tests | `Api/{Service}/Tests/{Endpoint}Tests.cs` |
| Param helper | `Api/Helpers/{Service}ParamHelper.cs` |
| Namespace (tests) | `Api.{Service}.Tests` |
| Namespace (models) | `Core.Models.{Service}` |
| Category (class) | `[Category("{Service}")]` |

Before generating — verify that `Core/Config/{Service}Endpoints.cs` exists. If not, create it.

## Flow

### Step 1: Research the endpoint

1. Fetch the endpoint from the API (use BaseUrl from `testsettings.json` → `TestConfig.{Service}BaseUrl`)
2. Analyze the response structure — what fields, what types, what's nullable
3. Check for sub-endpoints: `/{endpoint}/{id}`, nested resources
4. Count total items (needed for `ExpectedCount`)

### Step 2: Add constants to Endpoints file

Update `Core/Config/{Service}Endpoints.cs`:

Add:
- `Expected{Endpoint}Count` — total items from API
- `Test{Endpoint}Id` — valid ID for single-item tests (use ID = 1)
- `{Endpoint}` — path string (e.g. `/users`)
- `{Endpoint}ById` — path with segment (e.g. `/users/{id}`)

### Step 3: Generate Models

Create or update models in `Core/Models/{Service}/`:

**Main model** (`{Endpoint}Model.cs`):
- Suffix: `Model` for response, `Request` for request body
- Always includes `Id` field with `[PositiveId]` attribute
- Reference types (string, object, nested model): `[RequiredField]`, no `?`, no initializer
- Value types (int, decimal, double): `?` only if JSON field can be null/absent
- `[ValueRange(min, max)]` on numeric fields with known bounds (e.g. rating 0-5, price >= 0)
- Namespace: `Core.Models.{Service}`
- Pure data container — no constructors, no validation logic

**Attribute style** — always on separate line above property:
```csharp
[PositiveId]
public int Id { get; set; }

[RequiredField]
public string Title { get; set; }

[ValueRange(0, 5)]
public double Rate { get; set; }
```

**Nested models** — separate file for each nested object (e.g. `RatingModel`, `UserNameModel`, `AddressModel`).

**Check for shared base classes** — if 2+ models share identical fields with same types, create a base class in `Core/Models/Generic/` and inherit (e.g. `UserOwnedModel` for entities with `Id` + `UserId` + `Title`).

If models already exist — verify they match current API response.

### Step 4: Generate Tests

**Base class** — check if `{Service}RequestHelper` exists in `Core/Helpers/`. If not, create it extending `RequestHelper`. If it exists, use it.

**Param helpers** — use shared helpers for URL segments and query parameters:
- Use `using static Api.Helpers.{Service}ParamHelper;`
- If helper doesn't exist, create it with `ProductIdParam`/`UserIdParam`/`PostIdParam` as needed

For custom params, use `ParamType` enum (from `Core.Models`) — never magic strings:
```csharp
new() { Type = ParamType.UrlSegment, Key = "id", Value = id }
new() { Type = ParamType.Parameter, Key = "userId", Value = userId }
```

**Request body** — extract repeated bodies to `static readonly` fields:
```csharp
private static readonly PostModel TestPost = new() { UserId = 1, Title = "Test Post", Body = "Test Body" };
```

**Pattern:** Use `ShouldHaveValidFields()` via attributes — NOT per-field helpers.

```csharp
using NUnit.Framework;
using RestSharp;
using Core.Models.{Service};
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Api.Helpers.{Service}ParamHelper;

namespace Api.{Service}.Tests;

[TestFixture]
[AllureNUnit]
[Category("{Service}")]
public class GetAll{Endpoint}Tests : {BaseClass}
{
    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAll{Endpoint}_ReturnsOk()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint}, Method.Get);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.2 Content-Type is application/json")]
    public async Task GetAll{Endpoint}_ContentTypeIsJson()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint}, Method.Get);
        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Response body is not empty")]
    public async Task GetAll{Endpoint}_ReturnsNonEmptyList()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint}, Method.Get);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("1.4 Each item has valid required fields (via attributes)")]
    public async Task GetAll{Endpoint}_EachItemHasValidFields()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint}, Method.Get);
        foreach (var item in response.Data!)
        {
            item.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Performance")]
    [Description("1.5 Response time < 5 seconds")]
    public async Task GetAll{Endpoint}_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint}, Method.Get);
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan({Service}Endpoints.MaxResponseTimeMs);
    }

    [Test]
    [Category("Smoke")]
    [Description("1.6 Returns expected count")]
    public async Task GetAll{Endpoint}_ReturnsExpectedCount()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint}, Method.Get);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount({Service}Endpoints.Expected{Endpoint}Count);
    }
}
```

**For negative tests (non-existent ID):**
- Use dynamic approach: `GET all → maxId + 1`
- Never hardcode 999 or any static number
- If API returns 200 instead of 404 — use `[Ignore]` with explanation

### Step 5: Follow All Rules

- **Code:** PascalCase, file-scoped namespaces, async (`ExecuteAsync`), no magic numbers or strings
- **Categories:** `[Category("{Service}")]` on class; `[Category("HealthCheck|Smoke|Regression|Negative|Performance")]` on every method — see `Rules/categories.md`
- **Non-existent IDs:** Dynamic only — GET all → `maxId + 1`. Never static 999 or any hardcoded number
- **Assertions:** FluentAssertions only. Use `ShouldHaveValidFields()` via attributes — not per-field helpers
- **Attributes on separate lines above properties**, not inline
- **Config:** Use `{Service}Endpoints.*` from appropriate Endpoints file — never hardcode URLs
- **Comments:** No comments unless regex or non-obvious WHY
- **Models:** pure data containers, no constructors, no validation logic. Check for shared base classes
- **Negative tests:** `[Ignore]` attribute with explanation for API known bugs
- **Access modifiers:** narrowest possible — private > protected > public
- **Params:** Use `ParamType` enum for request parameters — never magic strings

### Step 6: Safety Check

Before finishing:
1. Run `dotnet format --verify-no-changes` — fix if needed
2. Run `dotnet test --filter "GetAll{Endpoint}"` — verify new tests pass
3. Show test results summary

### Step 7: Report

Show:
- Files created/modified
- Test count (total, positive, negative, edge cases)
- Any issues found during safety check
- Suggested commit message

---

## Test Run (Author)

**Input values used:**
- `{{service_name}}` = FakeStore
- `{{endpoint_name}}` = users

**Output quality:** Works — generates 12 tests (9 active, 3 ignored), models and constants match API response.

---

## Peer Review

**Reviewer:** MiMo (AI — playing teammate)
**Date reviewed:** 2026-06-25
**Model used by reviewer:** MiMo V2.5 Pro

**Reviewer input values used:**
- `{{service_name}}` = FakeStore
- `{{endpoint_name}}` = products

| Review question | Reviewer answer |
|---|---|
| Could you run the template without asking the author anything? | Yes — template contains all steps, file references, and rules. No questions needed. |
| Was the output format what you expected? | Yes — got Models, Endpoints, Tests in C# format as described. |
| Would you use this template on your own work? | Yes — works for any endpoint. |
| One concrete improvement suggestion | Add **Variable Placeholders** table section before Input — already added in v1.1. |

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-06-25 | Initial commit | Алексей |
| 1.1 | 2026-06-25 | Added Purpose, Variable Placeholders table, Output Format Instruction, Peer Review | Алексей |
| 1.2 | 2026-06-25 | Made service-agnostic: removed hardcoded FakeStore/JsonPlaceholder, added File Convention table, dynamic base class discovery | Алексей |
